using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Acuminator.Utilities;
using Acuminator.Vsix.Formatter;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Acuminator.Vsix.GoToDeclaration;
using Acuminator.Vsix.ServiceLocation;
using CommonServiceLocator;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using Acuminator.Vsix.Settings;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.ToolWindows.CodeMap;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utils;
using Acuminator.Utilities.DiagnosticSuppression;

using EnvDTE80;
using EnvDTE;
using System.Linq;



namespace Acuminator.Vsix
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.Debugging_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(AcuminatorVSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideOptionPage(typeof(GeneralOptionsPage), SettingsCategoryName, GeneralOptionsPage.PageTitle,
					   categoryResourceID: 201, pageNameResourceID: 202, supportsAutomation: true, SupportsProfiles = true)]
	[ProvideToolWindow(typeof(CodeMapWindow), MultiInstances = false, Transient = true, Orientation = ToolWindowOrientation.Left,
					   Style = VsDockStyle.Linked)]
	public sealed class AcuminatorVSPackage : AsyncPackage
	{
		private const int TotalLoadSteps = 5;
		private const string SettingsCategoryName = "Acuminator";

		/// <summary>
		/// AcuminatorVSPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "7e538ed0-0699-434f-acf0-3f6dbc9898ea";

		/// <summary>
		/// The acuminator default command set GUID string.
		/// </summary>
		public const string AcuminatorDefaultCommandSetGuidString = "3cd59430-1e8d-40af-b48d-9007624b3d77";

		[Import]
        internal IClassificationFormatMapService classificationFormatMapService = null;  //Set via MEF

        public IClassificationFormatMapService ClassificationFormatMapService => classificationFormatMapService;

        [Import]
        internal IClassificationTypeRegistryService classificationRegistry = null; // Set via MEF

        public IClassificationTypeRegistryService ClassificationRegistry => classificationRegistry;

        private const int INSTANCE_UNINITIALIZED = 0;
        private const int INSTANCE_INITIALIZED = 1;
        private static int instanceInitialized;

        public static AcuminatorVSPackage Instance { get; private set; }

        private object locker = new object();
		private SolutionEvents _dteSolutionEvents;


		private GeneralOptionsPage generalOptionsPage = null;     
      
        public GeneralOptionsPage GeneralOptionsPage
        {
            get
            {
                if (generalOptionsPage == null)
                {
                    lock (locker)
                    {
                        if (generalOptionsPage == null)
                        {
                            generalOptionsPage = GetDialogPage(typeof(GeneralOptionsPage)) as GeneralOptionsPage;
                        }
                    }
                }

                return generalOptionsPage;
            }
        }

		internal AcuminatorLogger AcuminatorLogger
		{
			get;
			private set;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AcuminatorVSPackage"/> class.
        /// </summary>
        public AcuminatorVSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        
            SetupSingleton(this);
        }
          
        private static void SetupSingleton(AcuminatorVSPackage package)
        {
            if (package == null)
                return;

            if (Interlocked.CompareExchange(ref instanceInitialized, INSTANCE_INITIALIZED, INSTANCE_UNINITIALIZED) == INSTANCE_UNINITIALIZED)
            {
                Instance = package;
            }
        }

		protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			// When initialized asynchronously, the current thread may be a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread
			await base.InitializeAsync(cancellationToken, progress);

			if (Zombied)
				return;

			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			await InitializeCommandsAsync(progress);		
			await SubscribeOnSolutionEventsAsync();
			cancellationToken.ThrowIfCancellationRequested();

			IComponentModel componentModel = Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;

			if (componentModel == null)
				return;

			InitializeLogger(progress);
			await InitializeSuppressionManagerAsync(progress);
			cancellationToken.ThrowIfCancellationRequested();

			var progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, VSIXResource.PackageLoad_InitCompositionContainer,
													   currentStep: 4, TotalLoadSteps);
			progress?.Report(progressData);

			try
			{
				componentModel.DefaultCompositionService.SatisfyImportsOnce(this);

				var container = new CompositionContainer(CompositionOptions.Default, componentModel.DefaultExportProvider);
				container.ComposeExportedValue<CodeAnalysisSettings>(new CodeAnalysisSettingsFromOptionsPage(GeneralOptionsPage));

				// Service Locator
				IServiceLocator serviceLocator = new MefServiceLocator(container);

				if (ServiceLocator.IsLocationProviderSet)
					serviceLocator = new DelegatingServiceLocator(ServiceLocator.Current, serviceLocator);

				ServiceLocator.SetLocatorProvider(() => serviceLocator);
			}
			catch
			{
				// Exception will be logged in FCEL
			}

			cancellationToken.ThrowIfCancellationRequested();
			progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, VSIXResource.PackageLoad_Done,
												   currentStep: 5, TotalLoadSteps);
			progress?.Report(progressData);
		}	

		private async System.Threading.Tasks.Task InitializeCommandsAsync(IProgress<ServiceProgressData> progress)
		{
			// if the package is zombied, we don't want to add commands
			if (Zombied)
				return;

			var progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, VSIXResource.PackageLoad_InitCommands,
													   currentStep: 1, TotalLoadSteps);
			progress?.Report(progressData);

			FormatBqlCommand.Initialize(this);
			GoToDeclarationOrHandlerCommand.Initialize(this);
			BqlFixer.FixBqlCommand.Initialize(this);

			OpenCodeMapWindowCommand.Initialize(this);
		}

		private async System.Threading.Tasks.Task SubscribeOnSolutionEventsAsync()
		{
			if (!ThreadHelper.CheckAccess())
				return;

#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
			DTE dte = await this.GetServiceAsync<DTE>();

			if (dte != null)
			{
				_dteSolutionEvents = dte.Events.SolutionEvents;						//Save DTE events object to prevent it from being GCed
				_dteSolutionEvents.AfterClosing += SolutionEvents_AfterClosing;
			}
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
		}

		private void SolutionEvents_AfterClosing()
		{
			CloseOpenToolWindows();
		}
		
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			AcuminatorLogger?.Dispose();

			if (ThreadHelper.CheckAccess() && _dteSolutionEvents != null)
			{		
				_dteSolutionEvents.AfterClosing -= SolutionEvents_AfterClosing;		
			}
		}

	    private void InitializeLogger(IProgress<ServiceProgressData> progress)
	    {
			var progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, VSIXResource.PackageLoad_InitLogger,
													   currentStep: 2, TotalLoadSteps);
			progress?.Report(progressData);

			try
		    {
			    AcuminatorLogger = new AcuminatorLogger(this, swallowUnobservedTaskExceptions: false);
		    }
		    catch (Exception ex)
		    {
			    ActivityLog.TryLogError(AcuminatorLogger.PackageName,
				    $"An error occurred during the logger initialization ({ex.GetType().Name}, message: \"{ex.Message}\")");
		    }
	    }

		private async System.Threading.Tasks.Task InitializeSuppressionManagerAsync(IProgress<ServiceProgressData> progress)
		{
			var progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, VSIXResource.PackageLoad_InitSuppressionManager,
													   currentStep: 3, TotalLoadSteps);
			progress?.Report(progressData);

			var workspace = await this.GetVSWorkspaceAsync();
			var additionalFiles = workspace.CurrentSolution.Projects
														   .SelectMany(p => p.AdditionalDocuments)
														   .Select(d => (path: d.FilePath, generateSuppressionBase: false));

			SuppressionManager.Init(new SuppressionFileSystemService(), additionalFiles);
		}

		protected override int QueryClose(out bool canClose)
		{
			CloseOpenToolWindows();
			return base.QueryClose(out canClose);
		}

		private void CloseOpenToolWindows()
		{
			if (!ThreadHelper.CheckAccess())
				return;

			try
			{
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
				DTE dte = GetService(typeof(DTE)) as DTE;
				dte?.Windows.Item($"{{{CodeMapWindow.CodeMapWindowGuidString}}}")?.Close();
#pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
			}
			catch
			{
			}
		}

		#region Package Settings         
		public bool ColoringEnabled => GeneralOptionsPage?.ColoringEnabled ?? true;


        public bool UseRegexColoring => GeneralOptionsPage?.UseRegexColoring ?? false;

        public bool UseBqlOutlining => GeneralOptionsPage?.UseBqlOutlining ?? true;

		public bool UseBqlDetailedOutlining => GeneralOptionsPage?.UseBqlDetailedOutlining ?? true;

        public bool PXGraphColoringEnabled => GeneralOptionsPage?.PXGraphColoringEnabled ?? true;
        
        public bool PXActionColoringEnabled => GeneralOptionsPage?.PXActionColoringEnabled ?? true;

        public bool ColorOnlyInsideBQL => GeneralOptionsPage?.ColorOnlyInsideBQL ?? false;
        #endregion
    }
}

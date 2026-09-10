#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Vsix.BannedApi;
using Acuminator.Vsix.CodeSnippets;
using Acuminator.Vsix.Coloriser;
using Acuminator.Vsix.DiagnosticSuppression;
using Acuminator.Vsix.Formatter;
using Acuminator.Vsix.GoToDeclaration;
using Acuminator.Vsix.Logger;
using Acuminator.Vsix.Settings;
using Acuminator.Vsix.ToolWindows.CodeMap;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Storage;

using Community.VisualStudio.Toolkit;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Threading;

using AcuminatorConstants = Acuminator.Vsix.Utilities.Constants;

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
	[InstalledProductRegistration(productName: "#110", productDetails: "#112", productId: PackageVersion, IconResourceID = 400)] // Info on this package for Help/About
	[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.Debugging_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(AcuminatorVSPackage.PackageGuidString)]
	[ProvideOptionPage(typeof(GeneralOptionsPage), SettingsCategoryName, GeneralOptionsPage.PageTitle,
					   categoryResourceID: 201, pageNameResourceID: 202, supportsAutomation: true, SupportsProfiles = true)]
	[ProvideOptionPage(typeof(CodeMapOptionsPage), SettingsCategoryName, CodeMapOptionsPage.PageTitle,
					   categoryResourceID: 201, pageNameResourceID: 203, supportsAutomation: true, SupportsProfiles = true)]
	[ProvideToolWindow(typeof(CodeMapWindow), MultiInstances = false, Transient = false, Orientation = ToolWindowOrientation.Left,
					   Style = VsDockStyle.Linked)]
	public sealed class AcuminatorVSPackage : AsyncPackage
	{
		private const string SettingsCategoryName = SharedConstants.PackageName;

		public const string PackageName = SharedConstants.PackageName;
		public const string PackageVersion = "4.1.0";

		/// <summary>
		/// AcuminatorVSPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "7e538ed0-0699-434f-acf0-3f6dbc9898ea";

		/// <summary>
		/// The acuminator default command set GUID string.
		/// </summary>
		public const string AcuminatorDefaultCommandSetGuidString = "3cd59430-1e8d-40af-b48d-9007624b3d77";

		private Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace? _vsWorkspace;

		private const int INSTANCE_UNINITIALIZED = 0;
		private const int INSTANCE_INITIALIZED = 1;
		private static int _instanceInitialized = INSTANCE_UNINITIALIZED;

		private const int NOT_DISPOSED = 0;
		private const int DISPOSED = 1;
		private volatile int _isDisposed = NOT_DISPOSED;

		private OutOfProcessSettingsUpdater? _outOfProcessSettingsUpdater;

		public static AcuminatorVSPackage Instance { get; private set; } = null!;


		/// <summary>
		/// The <see cref="JoinableTaskFactory"/> instance initialized for the <see cref="AsyncPackage"/>.<br/>
		/// If the package is not yet initialized or already disposed, <see cref="Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory"/> is returned instead.
		/// </summary>
		/// <remarks>
		/// According to VS cookbook and VS team's discussion, the <see cref="AsyncPackage.JoinableTaskFactory"/> should be preferred over <see cref="Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory"/>:
		/// <list type="bullet">
		/// <item>https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/issues/24</item>
		/// <item>https://microsoft.github.io/VSSDK-Analyzers/analyzers/VSSDK007.html</item>
		/// </list>
		/// Both factories are created from the same <see cref="JoinableTaskContext"/> — the one bound to the VS main thread.<br/>
		/// So, they have identical participation in the JTF dependency graph that prevents deadlocks on the UI thread. Swapping one for the other changes nothing about deadlock behavior.<br/>
		/// The difference is the <see cref="JoinableTaskCollection"/>. <see cref="AsyncPackage.JoinableTaskFactory"/> has its own collection, and package disposal drains it.<br/> 
		/// The work you started can't still be running against torn-down state after the package unloads.<br/>
		/// On the other hand, <see cref="Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory"/> is ambient and tracks nothing on your behalf. That's the reason behind VSSDK007 diagnostic.
		/// </remarks>
		public static JoinableTaskFactory JTF =>
			Instance?._isDisposed == NOT_DISPOSED
				? Instance.JoinableTaskFactory
				: ThreadHelper.JoinableTaskFactory;

		private readonly Lazy<GeneralOptionsPage?> _generalOptionsPage =
			new(() => Instance.GetDialogPage(typeof(GeneralOptionsPage)) as GeneralOptionsPage, isThreadSafe: true);

		public GeneralOptionsPage? GeneralOptionsPage => _generalOptionsPage.Value;

		private readonly Lazy<CodeMapOptionsPage?> _codeMapOptionsPage =
			new(() => Instance.GetDialogPage(typeof(CodeMapOptionsPage)) as CodeMapOptionsPage, isThreadSafe: true);

		public CodeMapOptionsPage? CodeMapOptionsPage => _codeMapOptionsPage.Value;

		internal AcuminatorLogger AcuminatorLogger
		{
			get;
			private set;
		} = null!;

		internal VSVersion VSVersion
		{
			get;
			private set;
		} = null!;

		internal AcuminatorMyDocumentsStorage? MyDocumentsStorage
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AcuminatorVSPackage"/> class.
		/// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public AcuminatorVSPackage()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.

			SetupSingleton(this);
		}
#pragma warning restore CS8618

		/// <summary>
		/// Force load package. 
		/// A hack method which is called from analyzers to ensure that the package is loaded before diagnostics are executed.
		/// </summary>
		/// <returns/>
		public static async System.Threading.Tasks.Task ForceLoadPackageAsync()
		{
			// In unit tests this can be null or throw NRE
			try
			{
				if (ThreadHelper.JoinableTaskFactory == null)
					return;
			}
			catch (Exception ex)
			{
				return;
			}

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			IVsShell? shell = await VS.GetServiceAsync<SVsShell, IVsShell>();

			if (shell == null)
				return;

			var packageToBeLoadedGuid = new Guid(PackageGuidString);
			shell.LoadPackage(ref packageToBeLoadedGuid, out var _);
			await System.Threading.Tasks.TaskScheduler.Default;
		}

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
		[MemberNotNull(nameof(Instance))]
		private static void SetupSingleton(AcuminatorVSPackage package)
		{
			if (Interlocked.CompareExchange(ref _instanceInitialized, INSTANCE_INITIALIZED, INSTANCE_UNINITIALIZED) == INSTANCE_UNINITIALIZED)
			{
				Instance = package;
			}
		}
#pragma warning restore CS8774

		protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, 
																			 IProgress<ServiceProgressData> progress)
		{
			// When initialized asynchronously, the current thread may be a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread
			await base.InitializeAsync(cancellationToken, progress);

			if (Zombied)
				return;

			try
			{
				await VS.StatusBar.StartAnimationAsync(StatusAnimation.General);

				await InitializeAcuminatorPackageAsync(cancellationToken, progress);
			}
			finally
			{
				await VS.StatusBar.EndAnimationAsync(StatusAnimation.General);
				await VS.StatusBar.ClearAsync();
			}
		}

		private async System.Threading.Tasks.Task InitializeAcuminatorPackageAsync(CancellationToken cancellationToken, 
																				   IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();

			#region Initialize Banned API
			cancellationToken.ThrowIfCancellationRequested();
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_DeployBannedApiFiles, currentStep: 1);

			MyDocumentsStorage = AcuminatorMyDocumentsStorage.TryInitialize(PackageVersion);
			var (deployedBannedApisFile, deployedAllowedApisFile) = DeployBannedApiFiles(MyDocumentsStorage);
			#endregion

			#region Initialize Settings
			cancellationToken.ThrowIfCancellationRequested();
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_InitCodeAnalysisSettings, currentStep: 2);

			_vsWorkspace = await this.GetVSWorkspaceAsync();

			await InitializePackageSettingsAsync(deployedBannedApisFile, deployedAllowedApisFile);
			#endregion

			#region Initialize Logger
			cancellationToken.ThrowIfCancellationRequested();
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_InitLogger, currentStep: 3);

			InitializeLogger();
			#endregion

			#region Initialize CodeSnippets
			cancellationToken.ThrowIfCancellationRequested();
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_InitCodeSnippets, currentStep: 4);

			DeployCodeSnippets(MyDocumentsStorage);
			#endregion

			#region Initialize Commands and SubscribeOnEvents	
			cancellationToken.ThrowIfCancellationRequested();
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_InitCommands, currentStep: 5);

			await InitializeCommandsAsync();
			SubscribeOnEvents();
			#endregion

			#region Suppression Manager Load
			await ReportProgressAsync(progress, VSIXResource.PackageLoad_InitSuppressionManager, currentStep: 6);
			cancellationToken.ThrowIfCancellationRequested();

			bool isSolutionOpen = await IsSolutionLoadedAsync();

			if (isSolutionOpen)
			{
				SetupSuppressionManager();
			}
			#endregion

			await ReportProgressAsync(progress, VSIXResource.PackageLoad_Done, currentStep: 7);
		}

		private async System.Threading.Tasks.Task ReportProgressAsync(IProgress<ServiceProgressData>? progress, string progressText, 
																	  int currentStep)
		{
			const int totalLoadSteps = 7;

			if (progress != null)
			{
				var progressData = new ServiceProgressData(VSIXResource.PackageLoad_WaitMessage, progressText, currentStep, totalLoadSteps);
				progress.Report(progressData);
			} 

			await VS.StatusBar.ShowMessageAsync(progressText);
		}

		private void SubscribeOnEvents()
		{
			SubscribeOnWorkspaceEvents();

			VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += SolutionEvents_OnAfterBackgroundSolutionLoadComplete;
		}

		private void SubscribeOnWorkspaceEvents()
		{
			if (_vsWorkspace != null)
			{
				_vsWorkspace.WorkspaceChanged += Workspace_WorkspaceChanged;
			}
		}

		private void InitializeLogger()
		{
			try
			{
				AcuminatorLogger = new AcuminatorLogger(this, swallowUnobservedTaskExceptions: false);
			}
			catch (Exception ex)
			{
				ActivityLog.TryLogError(PackageName,
					$"An error occurred during the logger initialization ({ex.GetType().Name}, message: \"{ex.Message}\")");
				throw;
			}
		}

		private async System.Threading.Tasks.Task InitializeCommandsAsync()
		{
			// if the package is zombied, we don't want to add commands
			if (Zombied)
				return;

			OleMenuCommandService? oleCommandService = await this.GetServiceAsync<IMenuCommandService, OleMenuCommandService>(throwOnFailure: false);

			if (oleCommandService == null)
			{
				InvalidOperationException loadCommandServiceException = new InvalidOperationException("Failed to load OLE command service");
				AcuminatorLogger.LogException(loadCommandServiceException, logOnlyFromAcuminatorAssemblies: false, LogMode.Error);
				return;
			}

			FormatBqlCommand.Initialize(this, oleCommandService);
			GoToDeclarationOrHandlerCommand.Initialize(this, oleCommandService);
			//BqlFixer.FixBqlCommand.Initialize(this, oleCommandService);

			OpenCodeMapWindowCommand.Initialize(this, oleCommandService);
		}

		private async System.Threading.Tasks.Task<bool> IsSolutionLoadedAsync()
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync();
			var solutionService = await this.GetServiceAsync<SVsSolution, IVsSolution>(throwOnFailure: false);

			if (solutionService == null)
				return false;

			int errorCode = solutionService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object isOpenProperty);
			ErrorHandler.ThrowOnFailure(errorCode);

			return isOpenProperty is bool isSolutionOpen && isSolutionOpen;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			// It is important to set flag after the base call to Dispose to avoid rare but possible VS hanging on package unload. 
			// The _isDisposed flag check on JTF property prevents returning JTF from a disposed package. But if the code flips it before base.Dispose call, there will be a problem. 
			// The base AsyncPackage.Dispose(bool) method does: disposeCancellationTokenSource.Cancel() -> ThreadHelper.JoinableTaskFactory.Run(JoinableTaskCollection.JoinTillEmptyAsync) — no token, no timeout —> Package.Dispose. 
			// So the main thread blocks until every JoinableTask in the package collection finishes.
			// During that drain, AcuminatorVSPackage.JTF already returns ThreadHelper's factory. A FileAndForgetAcuminatorTask wrapper started before shutdown is a member of the package collection (so the drain waits on it) 
			// and awaits a foreign task. When that foreign task resumes and needs its main-thread hop via AcuminatorVSPackage.JTF.SwitchToMainThreadAsync(), RequestSwitchToMainThread (with a null ambient job) creates a transient 
			// on ThreadHelper's factory — which has no collection, so the transient is not in the drained graph.A main thread blocked in Run pumps only joined work, 
			// so that continuation never runs -> the foreign task never completes -> the wrapper never completes -> JoinTillEmptyAsync never returns -> indefinite hang on close.
			//
			// Had the flag been set after base.Dispose, the same hop would go through the package factory, land in the collection, and be pumped by the drain — no hang. 
			// The "removed redundant cancellation tokens" commit compounds it: those switches no longer observe DisposalToken, so in-flight work can't self-cancel to escape the wait either.
			if (Interlocked.Exchange(ref _isDisposed, DISPOSED) == DISPOSED)
				return;

			AcuminatorLogger?.Dispose();
			_outOfProcessSettingsUpdater?.Dispose();

			VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete -= SolutionEvents_OnAfterBackgroundSolutionLoadComplete;

			if (_vsWorkspace != null)
			{
				_vsWorkspace.WorkspaceChanged -= Workspace_WorkspaceChanged;
				_vsWorkspace = null;
			}
		}

		private void SolutionEvents_OnAfterBackgroundSolutionLoadComplete()
		{
			SetupSuppressionManager();

			if (_vsWorkspace == null)
			{
				JoinableTaskFactory.Run(async () =>
				{
					await JoinableTaskFactory.SwitchToMainThreadAsync();

					_vsWorkspace = await this.GetVSWorkspaceAsync();
					SubscribeOnWorkspaceEvents();
					InitializeOutOfProcessSettingsSharing(GlobalSettings.AnalysisSettings, GlobalSettings.BannedApiSettings);
				});
			}
		}

		private void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
		{
			if (ShouldReloadSuppressionInfo(e))
			{
				SetupSuppressionManager();
			}
		}

		private bool ShouldReloadSuppressionInfo(WorkspaceChangeEventArgs e)
		{
			if (e.ProjectId == null)
				return false;

			switch (e.Kind)
			{
				case WorkspaceChangeKind.ProjectAdded:
				case WorkspaceChangeKind.ProjectRemoved:
				case WorkspaceChangeKind.ProjectChanged:
				case WorkspaceChangeKind.ProjectReloaded:
					break;

				default:
					return false;
			}

			HashSet<DocumentId> oldSuppressionFileIds = GetSuppressionFileIDs(e.OldSolution?.GetProject(e.ProjectId));
			HashSet<DocumentId> newSuppressionFileIds = GetSuppressionFileIDs(e.NewSolution?.GetProject(e.ProjectId));

			if (oldSuppressionFileIds.Count != newSuppressionFileIds.Count)
				return true;
			else if (oldSuppressionFileIds.Count == 0)
				return false;

			bool missingOldDocument = oldSuppressionFileIds.Any(oldDocId => !newSuppressionFileIds.Contains(oldDocId));
			bool addedNewDocument = newSuppressionFileIds.Any(newDocId => !newSuppressionFileIds.Contains(newDocId));

			return missingOldDocument || addedNewDocument;

			//---------------------------------Local function--------------------------------------------------------
			static HashSet<DocumentId> GetSuppressionFileIDs(Microsoft.CodeAnalysis.Project? project) =>
				project?.GetSuppressionFiles()
						.Select(file => file.Id)
						.ToHashSet() ?? [];
		}

		private void SetupSuppressionManager()
		{
			SuppressionManager.InitOrReset(_vsWorkspace, AcuminatorWorkMode.ReportUnsuppressedErrors, 
										   errorProcessorFabric: () => new VsixIOErrorProcessor(),
										   buildActionSetterFabric: () => SharedVsSettings.VSVersion?.VS2022OrNewer == true
																			? new VsixBuildActionSetterVS2022()
																			: new VsixBuildActionSetterVS2019());
		}

		private static (string? DeployedBannedApisFile, string? DeployedAllowedApisFile) DeployBannedApiFiles(
																							AcuminatorMyDocumentsStorage? myDocumentsStorage)
		{
			var bannedApiDeployer = BannedApiDeployer.Create(myDocumentsStorage);

			if (bannedApiDeployer == null)
			{
				AcuminatorLogger.LogMessage("Failed to create Banned API Deployer", LogMode.Warning);
				return default;
			}

			var (deployedBannedApisFile, deployedAllowedApisFile) = bannedApiDeployer.DeployBannedApiFiles();

			if (deployedBannedApisFile.IsNullOrWhiteSpace())
				AcuminatorLogger.LogMessage("Failed to initialize Banned APIs", LogMode.Warning);

			if (deployedAllowedApisFile.IsNullOrWhiteSpace())
				AcuminatorLogger.LogMessage("Failed to initialize Allowed APIs", LogMode.Warning);

			return (deployedBannedApisFile, deployedAllowedApisFile);
		}

		private async System.Threading.Tasks.Task InitializePackageSettingsAsync(string? deployedBannedApisFile, string? deployedAllowedApisFile)
		{
			CodeAnalysisSettings codeAnalysisSettings;
			BannedApiSettings bannedApiSettings;

			if (GeneralOptionsPage != null)
			{
				GeneralOptionsPage.SetDeployedBannedApiSettings(deployedBannedApisFile, deployedAllowedApisFile);

				codeAnalysisSettings = new CodeAnalysisSettingsFromOptionsPage(GeneralOptionsPage);
				bannedApiSettings	 = new BannedApiSettingsFromOptionsPage(GeneralOptionsPage);
			}
			else
			{
				codeAnalysisSettings = CodeAnalysisSettings.Default;
				bannedApiSettings	 = BannedApiSettings.Default;
			}

			GlobalSettings.InitializeGlobalSettingsOnce(codeAnalysisSettings, bannedApiSettings, AnalysisHostType.VisualStudio);

			VSVersion = await VSVersionProvider.GetVersionAsync(this);
			SharedVsSettings.VSVersion = VSVersion;

			InitializeOutOfProcessSettingsSharing(codeAnalysisSettings, bannedApiSettings);
		}

		private void InitializeOutOfProcessSettingsSharing(CodeAnalysisSettings initialCodeAnalysisSettings,
														   BannedApiSettings initialBannedApiSettings)
		{
			if (_outOfProcessSettingsUpdater != null || !this.IsOutOfProcessEnabled(_vsWorkspace) || GeneralOptionsPage == null)
				return;

			_outOfProcessSettingsUpdater = new OutOfProcessSettingsUpdater(GeneralOptionsPage, initialCodeAnalysisSettings, initialBannedApiSettings);
		}

		private static void DeployCodeSnippets(AcuminatorMyDocumentsStorage? myDocumentsStorage)
		{
			var codeSnippetsDeployer = CodeSnippetsDeployer.Create(myDocumentsStorage);

			if (codeSnippetsDeployer == null)
			{
				AcuminatorLogger.LogMessage("Failed to create Code Snippets Deployer", LogMode.Warning);
				return;
			}

			if (!codeSnippetsDeployer.DeployCodeSnippets())
			{
				AcuminatorLogger.LogMessage("Failed to initialize Code Snippets", LogMode.Warning);
			}
		}

		#region Package Settings
		public bool ColoringEnabled => GeneralOptionsPage?.ColoringEnabled ?? AcuminatorConstants.Settings.Coloring.ColoringEnabledDefault;

		public bool UseBqlOutlining => GeneralOptionsPage?.UseBqlOutlining ?? AcuminatorConstants.Settings.Outlining.UseBqlOutliningDefault;

		public bool UseBqlDetailedOutlining => 
			GeneralOptionsPage?.UseBqlDetailedOutlining ?? AcuminatorConstants.Settings.Outlining.UseBqlDetailedOutliningDefault;

		public bool PXGraphColoringEnabled => 
			GeneralOptionsPage?.PXGraphColoringEnabled ?? AcuminatorConstants.Settings.Coloring.PXGraphColoringEnabledDefault;

		public bool PXActionColoringEnabled => 
			GeneralOptionsPage?.PXActionColoringEnabled ?? AcuminatorConstants.Settings.Coloring.PXActionColoringEnabledDefault;

		public bool ColorOnlyInsideBQL => 
			GeneralOptionsPage?.ColorOnlyInsideBQL ?? AcuminatorConstants.Settings.Coloring.ColorOnlyInsideBQLDefault;

		public string? BannedApiFilePath => GeneralOptionsPage?.BannedApiFilePath;

		public string? AllowedApiFilePath => GeneralOptionsPage?.AllowedApiFilePath;

		public bool ExpandRootNodes => CodeMapOptionsPage?.ExpandRootNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandRootNodesDefault;

		public bool ExpandRegularNodes => CodeMapOptionsPage?.ExpandRegularNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandRegularNodesDefault;

		public bool ExpandAttributeNodes => CodeMapOptionsPage?.ExpandAttributeNodes ?? AcuminatorConstants.Settings.CodeMap.ExpandAttributeNodesDefault;
		#endregion
	}
}

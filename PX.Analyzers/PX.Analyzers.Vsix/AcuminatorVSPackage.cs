using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using PX.Analyzers.Vsix.Formatter;

namespace PX.Analyzers.Vsix
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
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	//[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndNotBuildingAndNotDebugging_string)] // Auto-load for dynamic menu enabling/disabling; this context seems to work for SSMS and VS
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(AcuminatorVSPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
                     Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideOptionPage(typeof(GeneralOptionsPage), AcuminatorVSPackage.SettingsCategoryName, GeneralOptionsPage.PageTitle, 0, 0, true)]
	public sealed class AcuminatorVSPackage : Package
    {
        private object locker = new object();
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

        public const string SettingsCategoryName = "Acuminator";
        /// <summary>
        /// AcuminatorVSPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "7e538ed0-0699-434f-acf0-3f6dbc9898ea";

        /// <summary>
        /// Initializes a new instance of the <see cref="AcuminatorVSPackage"/> class.
        /// </summary>
        public AcuminatorVSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

       

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
	        FormatBqlCommand.Initialize(this);
			base.Initialize();
        }

        #region Package Settings         
        public bool ColoringEnabled => GeneralOptionsPage?.ColoringEnabled ?? true;


        public bool UseRegexColoring => GeneralOptionsPage?.UseRegexColoring ?? false;

        public bool UseBqlOutlining => GeneralOptionsPage?.UseBqlOutlining ?? true;

        public bool PXGraphColoringEnabled => GeneralOptionsPage?.PXGraphColoringEnabled ?? true;
        
        public bool PXActionColoringEnabled => GeneralOptionsPage?.PXGraphColoringEnabled ?? true;
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Threading;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using PX.Analyzers.Vsix;



namespace PX.Analyzers.Coloriser
{
    public abstract class PXTaggerProviderBase
    {    
        protected bool IsInitialized { get; private set; }
        
        public AcuminatorVSPackage Package { get; protected set; }

        /// <summary>
        /// Initializes the reference to the Package if needed and sets the <see cref="IsInitialized"/> flag if all initialized correctly.
        /// </summary>
        protected virtual void Initialize()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;
            InitializePackage();
        }

        protected virtual void InitializePackage()
        {
            IVsShell shellService = ServiceProvider.GlobalProvider.GetService(typeof(IVsShell)) as IVsShell;

            if (shellService == null)
            {
                IsInitialized = false;
                return;
            }
            
            Guid acuminatorGUID = Guid.Parse(AcuminatorVSPackage.PackageGuidString);
            int returnCode = shellService.IsPackageLoaded(ref acuminatorGUID, out IVsPackage package);

            if (returnCode != Microsoft.VisualStudio.VSConstants.S_OK)
            {
                shellService.LoadPackage(ref acuminatorGUID, out package);
            }
            
            Package = package as AcuminatorVSPackage;

            if (Package == null)
                throw new Exception("Acuminator package loaded incorrectly");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using Acuminator.Vsix;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Acuminator.Vsix.Utilities;


using Path = System.IO.Path;



namespace Acuminator.Vsix.Coloriser
{
    public abstract class PXTaggerProviderBase
    {    
        protected bool IsInitialized { get; private set; }

        protected bool HasReferenceToAcumaticaPlatform { get; private set; }
        
        public AcuminatorVSPackage Package { get; protected set; }

        public Workspace Workspace { get; private set; }

        /// <summary>
        /// Initializes the reference to the Package if needed and sets the <see cref="IsInitialized"/> flag if all initialized correctly.
        /// </summary>
        protected virtual void Initialize(ITextBuffer buffer)
        {
            Workspace = buffer?.GetWorkspace();
            HasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica();

            if (IsInitialized)
                return;

            IsInitialized = true;
            InitializePackage();          
        }

        protected virtual void InitializePackage()
        {	
			if (Package != null)
                return;

			ThreadHelper.ThrowIfNotOnUIThread();
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
                throw new Exception("The Acuminator package was loaded incorrectly");
        }
      
        protected bool CheckIfCurrentSolutionHasReferenceToAcumatica()
        {
            if (Workspace?.CurrentSolution == null)
                return false;

            bool hasAcumaticaProjectsInSolution = Workspace.CurrentSolution.Projects.Any(project => IsAcumaticaAssemblyName(project.Name));

            if (hasAcumaticaProjectsInSolution)
                return true;
          
            bool hasMetadataRefs = (from project in Workspace.CurrentSolution.Projects
                                    from reference in project.MetadataReferences
                                    select Path.GetFileNameWithoutExtension(reference.Display))
                                   .Any(reference => IsAcumaticaAssemblyName(reference));

            return hasMetadataRefs;

            //*********************************************************************************************************************************
            bool IsAcumaticaAssemblyName(string dllName) => ColoringConstants.PlatformDllName == dllName ||
                                                            ColoringConstants.AppDllName == dllName;
        }
    }
}

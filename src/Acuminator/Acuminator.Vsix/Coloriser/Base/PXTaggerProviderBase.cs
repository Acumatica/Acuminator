using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

using Path = System.IO.Path;

namespace Acuminator.Vsix.Coloriser
{
    public abstract class PXTaggerProviderBase
    {    
        protected bool HasReferenceToAcumaticaPlatform { get; private set; }

        public Workspace Workspace { get; private set; }

        /// <summary>
        /// Initializes the base fields - <see cref="Workspace"/> and <see cref="HasReferenceToAcumaticaPlatform"/>.
        /// </summary>
        protected virtual void Initialize(ITextBuffer buffer)
        {
            Workspace = buffer?.GetWorkspace();
            HasReferenceToAcumaticaPlatform = CheckIfCurrentSolutionHasReferenceToAcumatica();
        }
      
        protected bool CheckIfCurrentSolutionHasReferenceToAcumatica()
        {
            if (Workspace?.CurrentSolution == null)
                return false;

            bool hasAcumaticaProjectsInSolution = 
				Workspace.CurrentSolution.Projects.Any(project => IsAcumaticaAssemblyName(project.Name) || 
																  IsAcumaticaAssemblyName(project.AssemblyName));
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

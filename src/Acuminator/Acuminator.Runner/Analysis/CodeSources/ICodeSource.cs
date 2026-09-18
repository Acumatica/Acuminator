using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Acuminator.Runner.Analysis.CodeSources
{
    /// <summary>
    /// Interface for code source.
    /// </summary>
    internal interface ICodeSource
    {
        CodeSourceType Type { get; }

        string Location { get; }

        Task<Solution> LoadSolutionAsync(MSBuildWorkspace workspace, CancellationToken cancellationToken);

        IEnumerable<Project> GetProjectsForValidation(Solution solution);
    }
}

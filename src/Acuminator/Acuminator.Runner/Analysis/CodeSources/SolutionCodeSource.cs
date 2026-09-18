using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Analysis.CodeSources
{
    internal class SolutionCodeSource : ICodeSource
    {
        public CodeSourceType Type => CodeSourceType.Solution;

        public string Location { get; }

        public SolutionCodeSource(string solutionPath)
        {
            Location = solutionPath.CheckIfNullOrWhiteSpace();
        }

		public Task<Solution> LoadSolutionAsync(MSBuildWorkspace workspace, CancellationToken cancellationToken) =>
			workspace.OpenSolutionAsync(Location, cancellationToken: cancellationToken);

        public IEnumerable<Project> GetProjectsForValidation(Solution solution) =>
            solution.CheckIfNull().Projects;
	}
}

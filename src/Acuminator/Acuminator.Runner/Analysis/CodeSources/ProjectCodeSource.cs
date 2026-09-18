using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

using Acuminator.Utilities.Common;

namespace Acuminator.Runner.Analysis.CodeSources
{
    internal class ProjectCodeSource : ICodeSource
    {
        public CodeSourceType Type => CodeSourceType.Project;

        public string Location { get; }

        public ProjectCodeSource(string projectPath)
        {
            Location = projectPath.CheckIfNullOrWhiteSpace();
        }

		public async Task<Solution> LoadSolutionAsync(MSBuildWorkspace workspace, CancellationToken cancellationToken)
		{
			Project project = await workspace.OpenProjectAsync(Location, cancellationToken: cancellationToken);
            return project.Solution;
		}

		public IEnumerable<Project> GetProjectsForValidation(Solution solution)
		{
            var project = solution.CheckIfNull().Projects.FirstOrDefault(p => p.FilePath == Location);
			return project != null ? [project] : [];
		}
	}
}

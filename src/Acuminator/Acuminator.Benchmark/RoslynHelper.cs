using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Acuminator.Benchmark
{
	internal static class RoslynHelper
	{
		public static ImmutableArray<DiagnosticAnalyzer> GetAvailableAnalyzers()
		{
			return Assembly.GetAssembly(typeof(PXDiagnosticAnalyzer)).ExportedTypes
				.Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
				.Select(Activator.CreateInstance)
				.Cast<DiagnosticAnalyzer>()
				.ToImmutableArray();
		}

		public static async Task<Compilation> GetCompilationAsync(string projectFilePath)
		{
			var workspace = MSBuildWorkspace.Create();
			Project project = await workspace.OpenProjectAsync(projectFilePath).ConfigureAwait(false);

			return await project.GetCompilationAsync().ConfigureAwait(false);
		}
	}
}

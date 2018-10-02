using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace Acuminator.Benchmark
{
	[MinColumn, MaxColumn]
	public class RoslynCodeAnalysis
	{
		private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = CollectAnalyzers();

		private readonly Compilation _compilation;

		public RoslynCodeAnalysis()
		{
			_compilation = GetCompilationAsync().Result;
		}

		private async Task<Compilation> GetCompilationAsync()
		{
			string projectFilePath = ConfigurationManager.AppSettings["ProjectFile"];
			var workspace = MSBuildWorkspace.Create();
			Project project = await workspace.OpenProjectAsync(projectFilePath).ConfigureAwait(false);

			return await project.GetCompilationAsync().ConfigureAwait(false);
		}

		[Benchmark]
		public async Task AnalyzeProject()
		{
			//var options = new CompilationWithAnalyzersOptions(null, null, true, true, false, null);
			//var compilationWithAnalyzers = _compilation.WithAnalyzers(Analyzers, options);
			var compilationWithAnalyzers = _compilation.WithAnalyzers(Analyzers);
			await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
		}

		private static ImmutableArray<DiagnosticAnalyzer> CollectAnalyzers()
		{
			return Assembly.GetAssembly(typeof(PXDiagnosticAnalyzer)).ExportedTypes
				.Where(t => !t.IsAbstract && typeof(DiagnosticAnalyzer).IsAssignableFrom(t))
				.Select(Activator.CreateInstance)
				.Cast<DiagnosticAnalyzer>()
				.ToImmutableArray();
		}
	}
}

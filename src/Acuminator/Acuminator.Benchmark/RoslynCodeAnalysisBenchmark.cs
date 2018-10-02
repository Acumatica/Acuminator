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
	[MemoryDiagnoser]
	[MinColumn, MaxColumn]
	public class RoslynCodeAnalysisBenchmark
	{
		private static readonly ImmutableArray<DiagnosticAnalyzer> Analyzers = RoslynHelper.GetAvailableAnalyzers();

		private readonly Compilation _compilation;

		public RoslynCodeAnalysisBenchmark()
		{
			string projectFilePath = ConfigurationManager.AppSettings["ProjectFile"];
			_compilation = RoslynHelper.GetCompilationAsync(projectFilePath).Result;
		}

		[Benchmark]
		public async Task AnalyzeProject()
		{
			var compilationWithAnalyzers = _compilation.WithAnalyzers(Analyzers);
			await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
		}
	}
}

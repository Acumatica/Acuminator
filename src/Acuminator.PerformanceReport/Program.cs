using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Benchmark;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;

// ReSharper disable LocalizableElement

namespace Acuminator.PerformanceReport
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				Console.WriteLine("No project file specified");
				return;
			}

			ImmutableArray<DiagnosticAnalyzer> analyzers = RoslynHelper.GetAvailableAnalyzers();
			Compilation compilation = RoslynHelper.GetCompilationAsync(args[0]).Result;

			var options = new CompilationWithAnalyzersOptions(null, null, 
				concurrentAnalysis: true, logAnalyzerExecutionTime: true,
				reportSuppressedDiagnostics: true, analyzerExceptionFilter: null);

			CompilationWithAnalyzers analyzedCompilation = compilation.WithAnalyzers(analyzers, options);
			var analysisResult = analyzedCompilation.GetAnalysisResultAsync(analyzers, CancellationToken.None).Result;
			
			foreach (var item in analysisResult.AnalyzerTelemetryInfo
				.OrderByDescending(kv => kv.Value.ExecutionTime))
			{
				DiagnosticAnalyzer analyzer = item.Key;
				AnalyzerTelemetryInfo info = item.Value;

				using (new InColor(ConsoleColor.Red))
					Console.Write("Analyzer: ");
				Console.WriteLine(analyzer.GetType().Name);
				
				using (new InColor(ConsoleColor.DarkGreen))
					Console.Write("Supported Diagnostics: ");
				Console.WriteLine(String.Join(", ", analyzer.SupportedDiagnostics.Select(d => d.Id).Distinct().OrderBy(id => id)));
				
				using (new InColor(ConsoleColor.Yellow))
					Console.Write("Execution Time: ");
				Console.WriteLine("{0:c}", info.ExecutionTime);

				Console.WriteLine();
			}
		}
	}

	class InColor : IDisposable
	{
		private readonly ConsoleColor _oldColor;

		public InColor(ConsoleColor color)
		{
			_oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
		}

		public void Dispose()
		{
			Console.ForegroundColor = _oldColor;
		}
	}
}

using Acuminator.Analyzers.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;

namespace Acuminator.Analyzers.StaticAnalysis
{
	public abstract class PXDiagnosticAnalyzer : DiagnosticAnalyzer
	{
		private static readonly object SuppressionLock = new object();

		protected static SuppressionManager SuppressionManager { get; set; }

		public override void Initialize(AnalysisContext context)
		{
			//context.EnableConcurrentExecution();

			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				InitSuppressionManager(compilationStartContext.Options);

				var pxContext = new PXContext(compilationStartContext.Compilation);

				if (ShouldAnalyze(pxContext))
				{
					AnalyzeCompilation(compilationStartContext, pxContext);
				}
			});
		}

		protected virtual bool ShouldAnalyze(PXContext pxContext) => pxContext.IsPlatformReferenced;

		internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext);

		private static void InitSuppressionManager(AnalyzerOptions options)
		{
			if (SuppressionManager == null)
			{
				lock (SuppressionLock)
				{
					if (SuppressionManager == null)
					{
						SuppressionManager = new SuppressionManager(options);
					}
				}
			}
		}

		public static void ReportDiagnosticWithSuppressionCheck(SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, Diagnostic diagnostic)
		{
			if (SuppressionManager.IsSuppressed(semanticModel, diagnostic))
			{
				return;
			}

			reportDiagnostic(diagnostic);
		}
	}
}

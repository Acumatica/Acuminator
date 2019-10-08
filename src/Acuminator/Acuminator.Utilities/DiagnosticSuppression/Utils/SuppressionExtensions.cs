using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public static class SuppressionExtensions
	{
		public static void ReportDiagnosticWithSuppressionCheck(this SymbolAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			var semanticModel = context.Compilation.GetSemanticModel(diagnostic.Location.SourceTree);

			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				semanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this SyntaxNodeAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this CodeBlockAnalysisContext context, Diagnostic diagnostic,
																CodeAnalysisSettings settings)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, settings, context.CancellationToken);
		}
	}
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Utilities.DiagnosticSuppression
{
	public static class ContextExtensions
	{
		public static void ReportDiagnosticWithSuppressionCheck(this SymbolAnalysisContext context,
			Diagnostic diagnostic)
		{
			var semanticModel = context.Compilation.GetSemanticModel(diagnostic.Location.SourceTree);

			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				semanticModel, context.ReportDiagnostic, diagnostic, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this SyntaxNodeAnalysisContext context,
			Diagnostic diagnostic)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, context.CancellationToken);
		}

		public static void ReportDiagnosticWithSuppressionCheck(this CodeBlockAnalysisContext context,
			Diagnostic diagnostic)
		{
			SuppressionManager.ReportDiagnosticWithSuppressionCheck(
				context.SemanticModel, context.ReportDiagnostic, diagnostic, context.CancellationToken);
		}
	}
}

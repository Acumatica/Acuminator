using System.IO;
using System.Text;
using System.Xml.Linq;
using Acuminator.Utilities.Common;
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

		public static string GetXDocumentStringWithDeclaration(this XDocument xDocument)
		{
			xDocument.ThrowOnNull(nameof(xDocument));

			var builder = new StringBuilder(capacity: 65);

			using (TextWriter writer = new Utf8StringWriter(builder))
			{
				xDocument.Save(writer);
			}

			return builder.ToString();
		}
	}
}

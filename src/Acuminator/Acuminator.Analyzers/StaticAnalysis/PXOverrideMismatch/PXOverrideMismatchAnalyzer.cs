using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class PXOverrideMismatchAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1096_PXOverrideMustMatchSignature);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type == GraphType.PXGraphExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel pxGraphExtension)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			pxGraphExtension.Symbol
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Where(m => !m.IsStatic && m.HasAttribute(pxContext.AttributeTypes.PXOverrideAttribute, false))
				.ForEach(m => AnalyzeMethod(context, pxContext, m));
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, IMethodSymbol methodSymbol)
		{
			// Here we know that the method is not static and has the correct attribute.

			var extensionType = methodSymbol.ContainingType.GetPXGraphExtension(pxContext.PXGraphExtensionType);
			var graphType = extensionType.GetFirstTypeArgument();

			if (graphType != null)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var allTypes = PXOverrideExtensions.GetAllBaseTypes(pxContext.PXGraphExtensionType, extensionType, graphType);

				context.CancellationToken.ThrowIfCancellationRequested();

				var hasMatchingMethod = allTypes
					.SelectMany(t => t.GetMethods(methodSymbol.Name))
					.Any(m => PXOverrideMethodSymbolComparer.Instance.Equals(m, methodSymbol));

				if (!hasMatchingMethod)
				{
					var diagnostic = Diagnostic.Create(
						Descriptors.PX1096_PXOverrideMustMatchSignature,
						methodSymbol.Locations.First());

					context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
				}
			}
		}
	}
}

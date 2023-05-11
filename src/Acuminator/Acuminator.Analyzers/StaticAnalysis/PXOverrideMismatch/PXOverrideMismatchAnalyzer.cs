#nullable enable

using System.Collections.Immutable;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Linq;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Common;
using System.Collections.Generic;

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

			var methodsWithPxOverrideAttribute = pxGraphExtension.Symbol
				.GetMembers()
				.OfType<IMethodSymbol>()
				.Where(m => !m.IsStatic && m.HasAttribute(pxContext.AttributeTypes.PXOverrideAttribute, false));

			if (methodsWithPxOverrideAttribute.Any())
			{
				var allBaseTypes = GetAllBaseTypesToBeChecked(pxGraphExtension.Symbol, pxContext.PXGraphExtension.Type);

				methodsWithPxOverrideAttribute.ForEach(m => AnalyzeMethod(context, pxContext, allBaseTypes, m!));
			}
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext, IEnumerable<INamedTypeSymbol> allBaseTypes, IMethodSymbol methodSymbol)
		{
			// Here we know that the method is not static and has the correct attribute.

			context.CancellationToken.ThrowIfCancellationRequested();

			var hasMatchingMethod = allBaseTypes
				.SelectMany(t => t.GetMethods(methodSymbol.Name))
				.Any(m => PXOverrideMethodSymbolComparer.Instance.Equals(m, methodSymbol));

			if (!hasMatchingMethod)
			{
				var location = methodSymbol.Locations.FirstOrDefault();

				if (location != null)
				{
					var diagnostic = Diagnostic.Create(Descriptors.PX1096_PXOverrideMustMatchSignature, location);

					context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static HashSet<INamedTypeSymbol> GetAllBaseTypesToBeChecked(INamedTypeSymbol containerType, INamedTypeSymbol? pxGraphExtensionType)
		{
			var allBaseTypes = new HashSet<INamedTypeSymbol>();

			if (pxGraphExtensionType == null)
			{
				return allBaseTypes;
			}

			var extensionType = containerType.GetPXGraphExtension(pxGraphExtensionType);
			var graphType = extensionType?.GetFirstTypeArgument();

			if (graphType != null)
			{
				extensionType!.GetBaseTypes(pxGraphExtensionType, allBaseTypes);
				graphType.GetBaseTypes(pxGraphExtensionType, allBaseTypes);
			}

			return allBaseTypes;
		}
	}
}

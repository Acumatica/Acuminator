using System.Collections.Immutable;
using System.Linq;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;


namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	public class DacUiAttributesAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			base.ShouldAnalyze(pxContext, dac) && 
			dac.DacType == DacType.Dac;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var dacAttributes = dac.Symbol.GetAttributes();
			var pxCacheNameAttribute = pxContext.AttributeTypes.PXCacheNameAttribute;
			var pxHiddenAttribute = pxContext.AttributeTypes.PXHiddenAttribute;
			bool hasPXCacheNameAttribute = false;
			bool hasPXHiddenAttribute = false;

			foreach (var attribute in dacAttributes.Where(a => a.AttributeClass != null))
			{
				if (attribute.AttributeClass.InheritsFromOrEquals(pxCacheNameAttribute))
				{
					hasPXCacheNameAttribute = true;
				}

				if (attribute.AttributeClass.InheritsFromOrEquals(pxHiddenAttribute))
				{
					hasPXHiddenAttribute = true;
				}

				if (hasPXCacheNameAttribute || hasPXHiddenAttribute)
					return;
			}

			var diagnostic = Diagnostic.Create(Descriptors.PX1094_DacShouldHaveUiAttribute,
											   dac.DacNode.Identifier.GetLocation());

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}

#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.DacUiAttributes
{
	public class DacUiAttributesAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1094_DacShouldHaveUiAttribute);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			base.ShouldAnalyze(pxContext, dac) && 
			dac.DacType == DacType.Dac && !dac.IsMappedCacheExtension;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			bool hasPXCacheNameAttribute = false;
			bool hasPXHiddenAttribute = false;

			foreach (var attributeInfo in dac.Attributes.Where(a => a.AttributeType != null))
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				hasPXCacheNameAttribute = hasPXCacheNameAttribute || attributeInfo.IsPXCacheName;
				hasPXHiddenAttribute	= hasPXHiddenAttribute || attributeInfo.IsPXHidden;

				if (hasPXCacheNameAttribute || hasPXHiddenAttribute)
					return;
			}

			var diagnostic = Diagnostic.Create(Descriptors.PX1094_DacShouldHaveUiAttribute,
											   dac.Node.Identifier.GetLocation());

			context.ReportDiagnosticWithSuppressionCheck(diagnostic, pxContext.CodeAnalysisSettings);
		}
	}
}

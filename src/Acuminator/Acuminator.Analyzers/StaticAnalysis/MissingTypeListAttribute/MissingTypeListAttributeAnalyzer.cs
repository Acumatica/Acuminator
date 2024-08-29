using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute
{
    public class MissingTypeListAttributeAnalyzer : DacAggregatedAnalyzerBase
	{
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacPropertyInfo property in dac.DeclaredDacFieldPropertiesWithAcumaticaAttributes)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeProperty(property, context, pxContext);
			}
		}

		private static void AnalyzeProperty(DacPropertyInfo property, SymbolAnalysisContext context, PXContext pxContext)
		{
			bool hasListAttribute = property.Attributes
											.SelectMany(aInfo => aInfo.FlattenedAcumaticaAttributes)
											.Any(flattenedAttribute => flattenedAttribute.Type.ImplementsInterface(pxContext.IPXLocalizableList));
			if (!hasListAttribute)
				return;

			bool hasTypeAttribute = property.Attributes
											.Any(aInfo => aInfo.AggregatedAttributeMetadata
															   .Any(attrMeta => attrMeta.IsFieldAttribute));
			if (!hasTypeAttribute)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer, property.Symbol.Locations.FirstOrDefault()),
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}
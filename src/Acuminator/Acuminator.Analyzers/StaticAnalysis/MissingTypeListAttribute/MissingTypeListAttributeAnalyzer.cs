using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute
{
    public class MissingTypeListAttributeAnalyzer : DacAggregatedAnalyzerBase
	{
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			var typeAttributesSet = GetTypeAttributesSet(pxContext);

			foreach (DacPropertyInfo property in dac.AllDeclaredProperties)
			{
				context.CancellationToken.ThrowIfCancellationRequested();
				AnalyzeProperty(property, context, pxContext, typeAttributesSet);
			}
		}

		private static void AnalyzeProperty(DacPropertyInfo property, SymbolAnalysisContext context, PXContext pxContext,
											IEnumerable<INamedTypeSymbol> typeAttributesSet)
		{
			var attributeTypes = property.Symbol.GetAttributes()
												.Select(a => a.AttributeClass)
												.ToList(capacity: 4);

			bool hasListAttribute = attributeTypes.Any(type => type.ImplementsInterface(pxContext.IPXLocalizableList));

			if (!hasListAttribute)
				return;

			//TODO we need to use FieldTypeAttributesRegister to perform complete analysis with consideration for aggregate attributes 
			bool hasTypeAttribute = attributeTypes.Any(propertyAttributeType => 
										typeAttributesSet.Any(typeAttribute => propertyAttributeType.InheritsFromOrEquals(typeAttribute)));
			if (!hasTypeAttribute)
			{
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer, property.Symbol.Locations.FirstOrDefault()),
					pxContext.CodeAnalysisSettings);
			}
		}

		private static IEnumerable<INamedTypeSymbol> GetTypeAttributesSet(PXContext pxContext) =>
			new INamedTypeSymbol[]
			{
				pxContext.FieldAttributes.PXIntAttribute,
				pxContext.FieldAttributes.PXShortAttribute,
				pxContext.FieldAttributes.PXStringAttribute,
				pxContext.FieldAttributes.PXByteAttribute,
				pxContext.FieldAttributes.PXDBDecimalAttribute,
				pxContext.FieldAttributes.PXDBDoubleAttribute,
				pxContext.FieldAttributes.PXDBIntAttribute,
				pxContext.FieldAttributes.PXDBShortAttribute,
				pxContext.FieldAttributes.PXDBStringAttribute,
				pxContext.FieldAttributes.PXDBByteAttribute,
				pxContext.FieldAttributes.PXDecimalAttribute,
				pxContext.FieldAttributes.PXDoubleAttribute
			};
	}
}
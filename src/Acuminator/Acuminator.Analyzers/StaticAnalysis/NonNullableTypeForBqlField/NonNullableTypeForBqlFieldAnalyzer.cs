using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField
{
	public class NonNullableTypeForBqlFieldAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1014_NonNullableTypeForBqlField);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacPropertyInfo property in dac.AllDeclaredProperties)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (!dac.FieldsByNames.TryGetValue(property.Name, out DacFieldInfo field))
					continue;

				if (property.Symbol.Type is INamedTypeSymbol propertyType && propertyType.IsValueType &&
					propertyType.ConstructedFrom?.SpecialType != SpecialType.System_Nullable_T)
				{
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1014_NonNullableTypeForBqlField, property.Symbol.Locations.First()),
						pxContext.CodeAnalysisSettings);
				}
			}			
		}
	}
}
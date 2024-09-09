using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldAnalyzer : DacAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1060_LegacyBqlField);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			pxContext.IsAcumatica2019R1_OrGreater && 
			base.ShouldAnalyze(pxContext, dac);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacBqlFieldInfo dacField in dac.DeclaredBqlFields)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (dacField.Symbol.BaseType?.SpecialType != SpecialType.System_Object ||
					dacField.Symbol.IsStronglyTypedBqlFieldOrBqlConstant(pxContext))
				{
					continue;
				}

				Location? location = dacField.Symbol.Locations.FirstOrDefault();

				if (location == null || !dac.PropertiesByNames.TryGetValue(dacField.Name, out DacPropertyInfo? property))
					continue;

				string propertyTypeName  = property.PropertyTypeUnwrappedNullable.GetSimplifiedName();
				var propertyDataTypeName = new DataTypeName(propertyTypeName);

				if (!DataTypeToBqlFieldTypeMapping.ContainsDataType(propertyDataTypeName))
					continue;

				var args = ImmutableDictionary.CreateBuilder<string, string?>();
				args.Add(DiagnosticProperty.PropertyType, propertyTypeName);
				context.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1060_LegacyBqlField, location, args.ToImmutable(), dacField.Name),
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}

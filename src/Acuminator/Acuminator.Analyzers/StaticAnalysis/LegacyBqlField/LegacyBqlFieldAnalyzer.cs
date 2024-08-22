
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldAnalyzer : DacAggregatedAnalyzerBase
	{
		public const string CorrespondingPropertyType = "CorrespondingPropertyType";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1060_LegacyBqlField);

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			pxContext.IsAcumatica2019R1_OrGreater && 
			base.ShouldAnalyze(pxContext, dac);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacBqlFieldInfo dacField in dac.DeclaredBqlFields)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (dacField.Symbol.BaseType.SpecialType != SpecialType.System_Object || AlreadyStronglyTyped(dacField.Symbol, pxContext))
					continue;

				Location? location = dacField.Symbol.Locations.FirstOrDefault();

				if (location == null || !dac.PropertiesByNames.TryGetValue(dacField.Name, out DacPropertyInfo property))
					continue;

					string? propertyTypeName = GetPropertyTypeName(property.Symbol, pxContext);

				if (propertyTypeName == null || !PropertyTypeToBqlFieldTypeMapping.ContainsPropertyType(propertyTypeName))
						continue;

					var args = ImmutableDictionary.CreateBuilder<string, string>();
					args.Add(CorrespondingPropertyType, propertyTypeName);
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1060_LegacyBqlField, location, args.ToImmutable(), dacField.Name),
						pxContext.CodeAnalysisSettings);
				}
			}

		internal static bool AlreadyStronglyTyped(INamedTypeSymbol dacFieldType, PXContext pxContext) =>
			dacFieldType.AllInterfaces.Any(t =>
				t.IsGenericType
				&& t.OriginalDefinition.Name == pxContext.IImplementType.Name
				&& t.TypeArguments.First().AllInterfaces.Any(z => z.Name == pxContext.BqlTypes.BqlDataType.Name));

		private static string? GetPropertyTypeName(IPropertySymbol property, PXContext pxContext) =>
			property.Type switch
			{
				IArrayTypeSymbol arrType                                        => arrType.ElementType.Name + "[]",
				INamedTypeSymbol namedType when namedType.IsNullable(pxContext) => namedType.GetUnderlyingTypeFromNullable(pxContext)?.Name,
				INamedTypeSymbol namedType                                      => namedType.Name,
				_                                                               => null,
			};
	}
}

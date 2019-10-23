using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Analyzers.StaticAnalysis.Dac;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	public class LegacyBqlFieldAnalyzer : DacAggregatedAnalyzerBase
	{
		public const string CorrespondingPropertyType = "CorrespondingPropertyType";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1060_LegacyBqlField);

		public static readonly ImmutableDictionary<string, string> PropertyTypeToFieldType = new Dictionary<string, string>
		{
			["String"] = "String",
			["Guid"] = "Guid",
			["DateTime"] = "DateTime",
			["Boolean"] = "Bool",
			["Byte"] = "Byte",
			["Int16"] = "Short",
			["Int32"] = "Int",
			["Int64"] = "Long",
			["Single"] = "Float",
			["Double"] = "Double",
			["Decimal"] = "Decimal",
			["Byte[]"] = "ByteArray",
		}.ToImmutableDictionary();

		public override bool ShouldAnalyze(PXContext pxContext, DacSemanticModel dac) =>
			pxContext.IsAcumatica2019R1 && 
			base.ShouldAnalyze(pxContext, dac);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, DacSemanticModel dac)
		{
			foreach (DacFieldInfo dacField in dac.DeclaredFields)
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				if (dacField.Symbol.BaseType.SpecialType != SpecialType.System_Object || AlreadyStronglyTyped(dacField.Symbol, pxContext))
					continue;

				Location location = dacField.Symbol.Locations.FirstOrDefault();

				if (location != null && dac.PropertiesByNames.TryGetValue(dacField.Name, out DacPropertyInfo property))
				{
					string propertyTypeName = GetPropertyTypeName(property.Symbol, pxContext);

					if (propertyTypeName == null || !PropertyTypeToFieldType.ContainsKey(propertyTypeName))
						continue;

					var args = ImmutableDictionary.CreateBuilder<string, string>();
					args.Add(CorrespondingPropertyType, propertyTypeName);
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1060_LegacyBqlField, location, args.ToImmutable(), dacField.Name),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		internal static bool AlreadyStronglyTyped(INamedTypeSymbol dacFieldType, PXContext pxContext) =>
			dacFieldType.AllInterfaces.Any(t =>
				t.IsGenericType
				&& t.OriginalDefinition.Name == pxContext.IImplementType.Name
				&& t.TypeArguments.First().AllInterfaces.Any(z => z.Name == pxContext.BqlTypes.BqlDataType.Name));

		private static string GetPropertyTypeName(IPropertySymbol property, PXContext pxContext)
		{
			switch (property.Type)
			{
				case IArrayTypeSymbol arrType:
					return arrType.ElementType.Name + "[]";

				case INamedTypeSymbol namedType when namedType.IsNullable(pxContext):
					return namedType.GetUnderlyingTypeFromNullable(pxContext)?.Name;

				case INamedTypeSymbol namedType:
					return namedType.Name;

				default:
					return null;
			}
		}
	}
}

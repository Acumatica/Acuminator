using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LegacyBqlFieldAnalyzer : PXDiagnosticAnalyzer
	{
		public const string CorrespondingPropertyType = "CorrespondingPropertyType";
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
			Descriptors.PX1060_LegacyBqlField);

	    protected override bool ShouldAnalyze(PXContext pxContext) => pxContext.IsAcumatica2019R1;

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(
				c => Analyze(c, pxContext),
				SymbolKind.NamedType);
		}

		private async void Analyze(SymbolAnalysisContext context, PXContext pxContext)
		{
			INamedTypeSymbol dacFieldType = context.Symbol as INamedTypeSymbol;
			if (!IsDacFieldType(dacFieldType) || AlreadyStronglyTyped(dacFieldType, pxContext)) return;

			var table = dacFieldType.ContainingType;
			if (table != null && table.ImplementsInterface(pxContext.IBqlTableType))
			{
				var property = table
					.GetBaseTypesAndThis()
					.SelectMany(t => t.GetMembers().OfType<IPropertySymbol>())
					.FirstOrDefault(f => !f.IsReadOnly && !f.IsWriteOnly && String.Equals(f.Name, dacFieldType.Name, StringComparison.OrdinalIgnoreCase));

				if (property != null)
				{
					string propertyTypeName = property.Type is IArrayTypeSymbol arrType
						? arrType.ElementType.Name+"[]"
						: property.Type is INamedTypeSymbol namedType
							? namedType.IsNullable(pxContext)
								? namedType.GetUnderlyingTypeFromNullable(pxContext).Name
								: namedType.Name
							: null;
					if (propertyTypeName == null || LegacyBqlFieldFix.PropertyTypeToFieldType.ContainsKey(propertyTypeName) == false)
						return;

					var properties = ImmutableDictionary.CreateBuilder<string, string>();
					properties.Add(CorrespondingPropertyType, propertyTypeName);
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1060_LegacyBqlField, dacFieldType.Locations.First(), properties.ToImmutable(), dacFieldType.Name));
				}
			}
		}

		private static bool IsDacFieldType(ITypeSymbol dacFieldType)
		{
			if (dacFieldType == null || dacFieldType.TypeKind != TypeKind.Class || !dacFieldType.IsDacField() && dacFieldType.ContainingType != null || dacFieldType.BaseType.SpecialType != SpecialType.System_Object)
				return false;

			return dacFieldType.ContainingType.IsDAC() || dacFieldType.ContainingType.IsDacExtension();
		}

		private static bool AlreadyStronglyTyped(INamedTypeSymbol dacFieldType, PXContext pxContext)
			=> dacFieldType.AllInterfaces.Any(t =>
				t.IsGenericType
				&& t.ConstructUnboundGenericType().Name == pxContext.IImplementType.Name
				&& t.TypeArguments.First().AllInterfaces.Any(z => z.Name == pxContext.BqlTypes.BqlDataType.Name));
	}
}
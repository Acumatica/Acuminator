using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.DiagnosticSuppression;

namespace Acuminator.Analyzers.StaticAnalysis.LegacyBqlField
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class LegacyBqlFieldAnalyzer : PXDiagnosticAnalyzer
	{
		public const string CorrespondingPropertyType = "CorrespondingPropertyType";
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.PX1060_LegacyBqlField);

		protected override bool ShouldAnalyze(PXContext pxContext) => pxContext.IsAcumatica2019R1;

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(
				c => Analyze(c, pxContext),
				SymbolKind.NamedType);
		}

		private void Analyze(SymbolAnalysisContext context, PXContext pxContext)
		{
			if (!(context.Symbol is INamedTypeSymbol dacOrDacExt) || !dacOrDacExt.IsDAC() && !dacOrDacExt.IsDacExtension() || context.CancellationToken.IsCancellationRequested)
				return;

			var bqlFields = dacOrDacExt.GetMembers().OfType<INamedTypeSymbol>().ToArray();
			var properties = dacOrDacExt.GetBaseTypesAndThis()
				.SelectMany(t => t.GetMembers().OfType<IPropertySymbol>())
				.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			foreach (INamedTypeSymbol dacFieldType in bqlFields)
			{
				if (context.CancellationToken.IsCancellationRequested)
					return;

				if (!IsDacFieldType(dacFieldType, pxContext) || AlreadyStronglyTyped(dacFieldType, pxContext))
					continue;

				Location location = dacFieldType.Locations.FirstOrDefault();
				if (location != null && properties.TryGetValue(dacFieldType.Name, out IPropertySymbol property))
				{
					string propertyTypeName = property.Type is IArrayTypeSymbol arrType
						? arrType.ElementType.Name + "[]"
						: property.Type is INamedTypeSymbol namedType
							? namedType.IsNullable(pxContext)
								? namedType.GetUnderlyingTypeFromNullable(pxContext).Name
								: namedType.Name
							: null;

					if (propertyTypeName == null || !LegacyBqlFieldFix.PropertyTypeToFieldType.ContainsKey(propertyTypeName) || context.CancellationToken.IsCancellationRequested)
						continue;

					var args = ImmutableDictionary.CreateBuilder<string, string>();
					args.Add(CorrespondingPropertyType, propertyTypeName);
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1060_LegacyBqlField, location, args.ToImmutable(), dacFieldType.Name),
						pxContext.CodeAnalysisSettings);
				}
			}
		}

		private static bool IsDacFieldType(ITypeSymbol dacFieldType, PXContext pxContext)
			=> dacFieldType.TypeKind == TypeKind.Class &&
				dacFieldType.IsDacField() &&
				dacFieldType.BaseType.SpecialType == SpecialType.System_Object &&
				dacFieldType.ContainingType != null &&
				dacFieldType.ContainingType.IsDacOrExtension(pxContext);

		internal static bool AlreadyStronglyTyped(INamedTypeSymbol dacFieldType, PXContext pxContext)
			=> dacFieldType.AllInterfaces.Any(t =>
				t.IsGenericType
				&& t.OriginalDefinition.Name == pxContext.IImplementType.Name
				&& t.TypeArguments.First().AllInterfaces.Any(z => z.Name == pxContext.BqlTypes.BqlDataType.Name));
	}
}

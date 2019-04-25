using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NonNullableTypeForBqlFieldAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1014_NonNullableTypeForBqlField);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeProperty(c, pxContext), SymbolKind.Property);
		}

		private static void AnalyzeProperty(SymbolAnalysisContext context, PXContext pxContext)
		{
			var property = (IPropertySymbol) context.Symbol;
			var parent = property.ContainingType;

			if (parent != null 
				&& (parent.ImplementsInterface(pxContext.IBqlTableType) || parent.InheritsFrom(pxContext.PXCacheExtensionType)))
			{
				var bqlField = parent.GetTypeMembers().FirstOrDefault(t => t.ImplementsInterface(pxContext.IBqlFieldType)
					&& String.Equals(t.Name, property.Name, StringComparison.OrdinalIgnoreCase));

				var propertyType = property.Type as INamedTypeSymbol;

				if (bqlField != null && propertyType != null
					&& propertyType.IsValueType && propertyType.ConstructedFrom?.SpecialType != SpecialType.System_Nullable_T)
				{
					context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(Descriptors.PX1014_NonNullableTypeForBqlField, property.Locations.First()),
						pxContext.CodeAnalysisSettings);
				}
			}
		}
	}
}
using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;

namespace Acuminator.Analyzers.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AttributeNotMatchingDacPropertyAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1021_PXDBFieldAttributeNotMatchingDacProperty);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeProperty(c, pxContext), SymbolKind.Property);
		}

		private static void AnalyzeProperty(SymbolAnalysisContext context, PXContext pxContext)
		{
            IPropertySymbol property = context.Symbol as IPropertySymbol;
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
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1014_NonNullableTypeForBqlField, property.Locations.First()));
				}
			}
		}
	}
}
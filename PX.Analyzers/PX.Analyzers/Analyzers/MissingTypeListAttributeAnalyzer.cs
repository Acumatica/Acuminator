using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Utilities;
using System.Collections.Generic;

namespace PX.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MissingTypeListAttributeAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Property);
		}

		private static void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			var property = (IPropertySymbol) context.Symbol;
            var parent = property.ContainingType;

            if (parent == null || !parent.InheritsFromOrEquals(pxContext.IBqlTableType, true))
                return;

            var lists = new List<INamedTypeSymbol> {
                                    //pxContext.PXBaseListAttributeType,
                                    pxContext.PXIntListAttributeType,
                                    pxContext.PXStringListAttributeType};

            var types = new List<INamedTypeSymbol> {
                                    pxContext.PXIntAttributeType,
                                    pxContext.PXShortAttributeType,
                                    pxContext.PXStringAttributeType,
                                    pxContext.PXByteAttributeType,
                                    pxContext.PXDBIntAttributeType,
                                    pxContext.PXDBShortAttributeType,
                                    pxContext.PXDBStringAttributeType,
                                    pxContext.PXDBByteAttributeType};

            var attributeClasses = property.GetAttributes().
                    Select(a => a.AttributeClass);

            var listAttribute = attributeClasses.
                    FirstOrDefault(c => lists.Any(l => c.InheritsFromOrEquals(l, true)));

            if (listAttribute == null)
                return;

            //hardcode
            bool hasTypeAttribute = attributeClasses.
                    Any(c => types.Any(l => c.InheritsFromOrEquals(l, true)));

            if(!hasTypeAttribute)
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer, property.Locations.First()));

        }
	}
}
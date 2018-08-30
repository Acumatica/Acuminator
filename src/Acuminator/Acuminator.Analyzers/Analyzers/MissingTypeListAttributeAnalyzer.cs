using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;


namespace Acuminator.Analyzers
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
            var types = new List<INamedTypeSymbol>
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


            var attributeClasses = property.GetAttributes().
                    Select(a => a.AttributeClass);

            var listAttribute = attributeClasses.
                    FirstOrDefault(c => c.InheritsFromOrEquals(pxContext.IPXLocalizableList, true));

            if (listAttribute == null)
                return;

            var systemObject = context.Compilation.GetTypeByMetadataName(typeof(System.Object).FullName);
            while (!listAttribute.BaseType.Equals(pxContext.AttributeTypes.PXEventSubscriberAttribute) &&
                   !listAttribute.BaseType.Equals(systemObject))
                listAttribute = listAttribute.BaseType;
            //hardcode
            bool hasTypeAttribute = attributeClasses.
                    Any(c => types.Any(l => c.InheritsFromOrEquals(l, true)));

            if(!hasTypeAttribute)
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1002_MissingTypeListAttributeAnalyzer, property.Locations.First()));

        }
	}
}
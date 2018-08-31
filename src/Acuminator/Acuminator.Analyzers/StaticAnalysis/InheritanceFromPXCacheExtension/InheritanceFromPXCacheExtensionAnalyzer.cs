using System;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class InheritanceFromPXCacheExtensionAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1009_InheritanceFromPXCacheExtension,
				Descriptors.PX1011_InheritanceFromPXCacheExtension);
        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(c => Analyze(c, pxContext), 
                SymbolKind.NamedType);
        }

        private void Analyze(SymbolAnalysisContext context, PXContext pxContext)
        {
	        var symbol = (INamedTypeSymbol) context.Symbol;
	        if (!symbol.InheritsFrom(pxContext.PXCacheExtensionType)
	            || String.Equals(nameof(PXCacheExtension), symbol.Name, StringComparison.Ordinal)
				|| symbol.InheritsFromOrEquals(pxContext.PXMappedCacheExtensionType))
	        {
		        return;
	        }

	        if (String.Equals(nameof(PXCacheExtension), symbol.BaseType.Name, StringComparison.Ordinal))
	        {
		        if (!symbol.IsSealed)
		        {
			        context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1011_InheritanceFromPXCacheExtension, symbol.Locations.First()));
		        }
	        }
	        else
	        {
		        context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1009_InheritanceFromPXCacheExtension, symbol.Locations.First()));
	        }
        }
    }
}

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using PX.Analyzers.Utilities;
using System.Collections.Generic;

namespace PX.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ViewDeclarationOrderAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1004_ViewDeclarationOrder, Descriptors.PX1006_ViewDeclarationOrder);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.NamedType);
		}

		private static void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			var graph = (INamedTypeSymbol)context.Symbol;
            if (!graph.InheritsFromOrEquals(pxContext.PXGraphType) &&
                !graph.InheritsFromOrEquals(pxContext.PXGraphExtensionType))
                return;

            var test = graph.GetMembers().Where(m => m is IFieldSymbol);
            var selects = graph.GetMembers().Where(m => m is IFieldSymbol).
                Select(f => ((IFieldSymbol)f).Type as INamedTypeSymbol).
                Where(t => t != null &&
                           t.InheritsFrom(pxContext.PXSelectBaseType) && 
                           t.IsGenericType &&
                           t.TypeArguments.Count() > 0).
                ToImmutableList();

            var visited = new HashSet<INamedTypeSymbol>();
            //forward pass
            foreach (var select in selects)
            {
                var type = (INamedTypeSymbol)select.TypeArguments.First();
                if (visited.Contains(type))
                    continue;
                foreach(var parent in type.GetBaseTypes())
                    if(visited.Contains(parent))
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1004_ViewDeclarationOrder, graph.Locations.First()));
                visited.Add(type);
            }

            //backward pass
            visited.Clear();
            foreach (var select in selects.Reverse())
            {
                var type = (INamedTypeSymbol)select.TypeArguments.First();
                if (visited.Contains(type))
                    continue;
                foreach (var parent in type.GetBaseTypes())
                    if (visited.Contains(parent))
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1006_ViewDeclarationOrder, graph.Locations.First()));
                visited.Add(type);
            }
        }
	}
}
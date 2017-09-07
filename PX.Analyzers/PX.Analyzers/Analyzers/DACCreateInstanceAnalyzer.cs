using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DACCreateInstanceAnalyzer : PXDiagnosticAnalyzer
    {
        private class Walker : CSharpSyntaxWalker
        {
            private SymbolAnalysisContext _context;
            private readonly PXContext _pxContext;
            private readonly SemanticModel _semanticModel;

            public Walker(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
            {
                _context = context;
                _pxContext = pxContext;
                _semanticModel = semanticModel;
            }

	        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
	        {
		        if (node.Type != null)
		        {
			        var typeSymbol = _semanticModel.GetSymbolInfo(node.Type).Symbol as ITypeSymbol;
			        if (typeSymbol != null && typeSymbol.InheritsFromOrEquals(_pxContext.IBqlTableType, true))
			        {
				        _context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1007_DACCreateInstance, node.GetLocation(), typeSymbol.Name));
			        }
		        }

		        base.VisitObjectCreationExpression(node);
	        }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1007_DACCreateInstance);
        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(c => Analyze(c, pxContext), 
                SymbolKind.Method, SymbolKind.Field);
        }

        private async void Analyze(SymbolAnalysisContext context, PXContext pxContext)
        {
            var symbol = context.Symbol;
	        var declaration = symbol.DeclaringSyntaxReferences[0];
			var syntaxTree = await declaration.GetSyntaxAsync(context.CancellationToken).ConfigureAwait(false);
            var semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);
            var walker = new Walker(context, pxContext, semanticModel);
            walker.Visit(syntaxTree);
        }
    }
}

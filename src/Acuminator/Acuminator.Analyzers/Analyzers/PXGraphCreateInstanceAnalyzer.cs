using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;


namespace Acuminator.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PXGraphCreateInstanceAnalyzer : PXDiagnosticAnalyzer
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
                if (node.Type == null || !(_semanticModel.GetSymbolInfo(node.Type).Symbol is ITypeSymbol typeSymbol))
                {
                    base.VisitObjectCreationExpression(node);
                    return;
                }

                DiagnosticDescriptor descriptor = null;

                if (typeSymbol.InheritsFrom(_pxContext.PXGraphType))
                {
                    descriptor = Descriptors.PX1001_PXGraphCreateInstance;
                }
                else if (typeSymbol.Equals(_pxContext.PXGraphType))
                {
                    descriptor = Descriptors.PX1003_NonSpecificPXGraphCreateInstance;
                }

                if (descriptor != null)
                {
                    _context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation()));
                }

                base.VisitObjectCreationExpression(node);
            }
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1001_PXGraphCreateInstance,
				Descriptors.PX1003_NonSpecificPXGraphCreateInstance);

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

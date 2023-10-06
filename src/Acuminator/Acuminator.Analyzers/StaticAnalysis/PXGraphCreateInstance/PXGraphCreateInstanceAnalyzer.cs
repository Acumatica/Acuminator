#nullable enable

using System.Collections.Immutable;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreateInstance
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
                if (node.Type == null || _semanticModel.GetSymbolInfo(node.Type).Symbol is not ITypeSymbol typeSymbol)
                {
                    base.VisitObjectCreationExpression(node);
                    return;
                }

                DiagnosticDescriptor? descriptor = GetDiagnosticDescriptor(typeSymbol);

                if (descriptor != null)
                {
                    _context.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(descriptor, node.GetLocation()), 
						_pxContext.CodeAnalysisSettings);
                }

                base.VisitObjectCreationExpression(node);
            }

			private DiagnosticDescriptor? GetDiagnosticDescriptor(ITypeSymbol typeSymbol)
			{
				if (typeSymbol is ITypeParameterSymbol typeParameterSymbol && typeParameterSymbol.IsPXGraph(_pxContext))
				{
					return Descriptors.PX1001_PXGraphCreateInstance;
				}
				else if (typeSymbol.InheritsFrom(_pxContext.PXGraph.Type!))
				{
					return Descriptors.PX1001_PXGraphCreateInstance;
				}
				else if (typeSymbol.Equals(_pxContext.PXGraph.Type))
				{
					return Descriptors.PX1003_NonSpecificPXGraphCreateInstance;
				}
				
				return null;
			}
		}

        public PXGraphCreateInstanceAnalyzer() : this(null)
        { }

        public PXGraphCreateInstanceAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
        { }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
				Descriptors.PX1001_PXGraphCreateInstance,
				Descriptors.PX1003_NonSpecificPXGraphCreateInstance);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSymbolAction(c => AnalyzeGraphCreation(c, pxContext), 
                SymbolKind.Method, SymbolKind.Field, SymbolKind.Property);
        }

        private void AnalyzeGraphCreation(SymbolAnalysisContext context, PXContext pxContext)
        {
			context.CancellationToken.ThrowIfCancellationRequested();

            var symbol = context.Symbol;

			if (symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty || symbol.DeclaringSyntaxReferences[0] is not { } declaration)
				return;

			SyntaxNode? symbolNode = declaration.GetSyntax(context.CancellationToken);

			if (symbolNode == null)
				return;

            SemanticModel? semanticModel = context.Compilation.GetSemanticModel(declaration.SyntaxTree);

			if (semanticModel == null)
				return;

            var walker = new Walker(context, pxContext, semanticModel);
            walker.Visit(symbolNode);
        }
    }
}

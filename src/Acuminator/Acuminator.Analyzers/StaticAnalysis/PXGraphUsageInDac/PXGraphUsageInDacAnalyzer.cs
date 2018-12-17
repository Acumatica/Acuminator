using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class PXGraphUsageInDacAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create
            (
                Descriptors.PX1029_PXGraphUsageInDac
            );

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeGraphUsageInDac(syntaxContext, pxContext), SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeGraphUsageInDac(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (!(syntaxContext.Node is ClassDeclarationSyntax classDeclaration) || syntaxContext.CancellationToken.IsCancellationRequested)
                return;

            INamedTypeSymbol classType = syntaxContext.SemanticModel.GetDeclaredSymbol(classDeclaration, syntaxContext.CancellationToken);
            if (classType == null || !classType.IsDacOrExtension(pxContext))
                return;

            GraphUsageInDacWalker walker = new GraphUsageInDacWalker(syntaxContext, pxContext);
            walker.Visit(classDeclaration);
        }

        private class GraphUsageInDacWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext _syntaxContext;
            private readonly PXContext _pxContext;
            private bool _inDac;

            public GraphUsageInDacWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
            {
                _syntaxContext = syntaxContext;
                _pxContext = pxContext;
            }

	        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
	        {
		        ThrowIfCancellationRequested();

		        INamedTypeSymbol symbol = _syntaxContext.SemanticModel.GetDeclaredSymbol(node, _syntaxContext.CancellationToken);
		        if (symbol != null && symbol.IsDacOrExtension(_pxContext) && !_inDac)
		        {
                    _inDac = true;

                    base.VisitClassDeclaration(node);

                    _inDac = false;
				}
	        }

	        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                ThrowIfCancellationRequested();

                if (node.IsStatic())
                    return;

                base.VisitMethodDeclaration(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
				ThrowIfCancellationRequested();

				SymbolInfo symbolInfo = _syntaxContext.SemanticModel.GetSymbolInfo(node, _syntaxContext.CancellationToken);

                if (symbolInfo.Symbol == null || (symbolInfo.Symbol.Kind == SymbolKind.Method && symbolInfo.Symbol.IsStatic))
                    return;

                base.VisitInvocationExpression(node);
            }

            public override void VisitAttributeList(AttributeListSyntax node)
            {
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
				ThrowIfCancellationRequested();

				TypeInfo typeInfo = _syntaxContext.SemanticModel.GetTypeInfo(node, _syntaxContext.CancellationToken);

                if (typeInfo.Type == null || !typeInfo.Type.IsPXGraphOrExtension(_pxContext))
                    return;

                _syntaxContext.ReportDiagnosticWithSuppressionCheck(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDac, node.GetLocation()));

                base.VisitIdentifierName(node);
            }

	        private void ThrowIfCancellationRequested()
	        {
				_syntaxContext.CancellationToken.ThrowIfCancellationRequested();
			}
        }
    }
}

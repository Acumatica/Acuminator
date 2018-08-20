using Acuminator.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacGraphUsageAnalyzer : PXDiagnosticAnalyzer
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

            public GraphUsageInDacWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
            {
                _syntaxContext = syntaxContext;
                _pxContext = pxContext;
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (node.IsStatic() || _syntaxContext.CancellationToken.IsCancellationRequested)
                    return;

                base.VisitMethodDeclaration(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (_syntaxContext.CancellationToken.IsCancellationRequested)
                    return;

                SymbolInfo symbolInfo = _syntaxContext.SemanticModel.GetSymbolInfo(node, _syntaxContext.CancellationToken);

                if (symbolInfo.Symbol == null || (symbolInfo.Symbol.Kind == SymbolKind.Method && symbolInfo.Symbol.IsStatic))
                    return;

                base.VisitInvocationExpression(node);
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (_syntaxContext.CancellationToken.IsCancellationRequested)
                    return;

                TypeInfo typeInfo = _syntaxContext.SemanticModel.GetTypeInfo(node, _syntaxContext.CancellationToken);

                if (typeInfo.Type == null || !typeInfo.Type.IsPXGraphOrExtension(_pxContext))
                    return;

                _syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1029_PXGraphUsageInDac, node.GetLocation()));
            }
        }
    }
}

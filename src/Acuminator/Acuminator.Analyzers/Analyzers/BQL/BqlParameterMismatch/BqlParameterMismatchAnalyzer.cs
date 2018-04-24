using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using PX.Data;


namespace Acuminator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1015_PXBqlParametersMismatch);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeNode(c, pxContext), SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
        {
            if (syntaxContext.CancellationToken.IsCancellationRequested || !(syntaxContext.Node is InvocationExpressionSyntax invocationNode))
                return;

            var symbolInfo = syntaxContext.SemanticModel.GetSymbolInfo(invocationNode);

            if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol) || !IsValidMethodGeneralCheck(methodSymbol, pxContext))
                return;

            if (methodSymbol.IsStatic)
                AnalyzeStaticInvocation(methodSymbol, pxContext, syntaxContext, invocationNode);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsValidMethodGeneralCheck(IMethodSymbol methodSymbol, PXContext pxContext)
        {
            if (methodSymbol.IsExtern)
                return false;

            switch (methodSymbol.MethodKind)
            {
                case MethodKind.DelegateInvoke:
                case MethodKind.Ordinary:
                case MethodKind.DeclareMethod:
                    return IsValidReturnType(methodSymbol, pxContext);
                default:
                    return false;
            }
        }

        private static bool IsValidReturnType(IMethodSymbol methodSymbol, PXContext pxContext)
        {
            if (methodSymbol.ReturnsVoid)
                return false;

            var returnType = methodSymbol.ReturnType;
            return returnType.ImplementsInterface(pxContext.IPXResultsetType) ||
                   returnType.InheritsFrom(pxContext.PXResult) ||
                   returnType.ImplementsInterface(pxContext.IBqlTableType);
        }

        private static void AnalyzeStaticInvocation(IMethodSymbol methodSymbol, PXContext pxContext, SyntaxNodeAnalysisContext syntaxContext,
                                                    InvocationExpressionSyntax invocationNode)
        {
            if (!methodSymbol.ContainingType.IsBqlCommand())
                return;

            StaticMismatchWalker walker = new StaticMismatchWalker(syntaxContext, pxContext);
            walker.Visit(invocationNode);
            int argsCount = invocationNode.ArgumentList.Arguments.Count;
            int maxCount = walker.OptionalParametersCount + walker.RequiredParametersCount;
            int minCount = walker.RequiredParametersCount;
            
            if (argsCount < minCount || argsCount > maxCount)
            {
                Location location = GetLocation(invocationNode);
                syntaxContext.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1015_PXBqlParametersMismatch, location));
            }           
        }

        private static Location GetLocation(InvocationExpressionSyntax invocationNode)
        {
            var memberAccessNode = invocationNode.ChildNodes().OfType<MemberAccessExpressionSyntax>()
                                                              .FirstOrDefault();

            if (memberAccessNode == null)
                return invocationNode.GetLocation();

            var methodIdentifierNode = memberAccessNode.ChildNodes().OfType<IdentifierNameSyntax>()
                                                                    .LastOrDefault();

            return methodIdentifierNode?.GetLocation() ?? invocationNode.GetLocation();
        }

        private class StaticMismatchWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext syntaxContext;
            private readonly PXContext pxContext;
            private readonly SemanticModel semanticModel;
            private readonly CancellationToken cancellationToken;
            
            public int RequiredParametersCount
            {
                get;
                private set;
            }

            public int OptionalParametersCount
            {
                get;
                private set;
            }

            public StaticMismatchWalker(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext)
            {
                syntaxContext = aSyntaxContext;
                pxContext = aPxContext;
                semanticModel = syntaxContext.SemanticModel;
                cancellationToken = syntaxContext.CancellationToken;
            }

            public override void VisitGenericName(GenericNameSyntax genericNode)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(genericNode);

                if (cancellationToken.IsCancellationRequested || !(symbolInfo.Symbol is ITypeSymbol typeSymbol))
                {
                    if (!cancellationToken.IsCancellationRequested)
                        base.VisitGenericName(genericNode);

                    return;
                }

                if (genericNode.IsUnboundGenericName)
                    typeSymbol = typeSymbol.OriginalDefinition;

                PXCodeType? codeType = typeSymbol.GetCodeTypeFromGenericName();

                if (codeType == PXCodeType.BqlParameter)
                {
                    if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Required))
                        RequiredParametersCount++;
                    else if (typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional) || typeSymbol.InheritsFromOrEquals(pxContext.BQL.Optional2))
                        OptionalParametersCount++;
                }

                if (!cancellationToken.IsCancellationRequested)
                    base.VisitGenericName(genericNode);             
            }
        }
    }
}

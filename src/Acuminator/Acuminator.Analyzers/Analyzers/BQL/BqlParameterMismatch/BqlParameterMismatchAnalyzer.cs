using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
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
                AnalyzeStaticInvocation(methodSymbol, pxContext);

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

        private static void AnalyzeStaticInvocation(IMethodSymbol methodSymbol, PXContext pxContext)
        {
            if (!methodSymbol.ContainingType.IsBqlCommand())
                return;

            
        }
    }
}

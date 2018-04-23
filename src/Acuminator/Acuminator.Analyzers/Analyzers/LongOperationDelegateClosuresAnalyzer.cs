using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities;


namespace Acuminator.Analyzers.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeMethod(c, pxContext), SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeMethod(SyntaxNodeAnalysisContext context, PXContext pxContext)
		{
			var setDelegateInvocation = (InvocationExpressionSyntax)context.Node;
            var setDelegatememberAccessExpr = setDelegateInvocation.Expression as MemberAccessExpressionSyntax;

            if (setDelegatememberAccessExpr?.Name.ToString() != "SetProcessDelegate")
                return;

            var setDelegateSymbol = context.SemanticModel.GetSymbolInfo(setDelegatememberAccessExpr).Symbol as IMethodSymbol;

            if (!setDelegateSymbol.ContainingType.ConstructedFrom.InheritsFromOrEquals(pxContext.PXProcessingBaseType))
                return;
            
            DataFlowAnalysis dfa = null;

            switch (setDelegateInvocation.ArgumentList.Arguments[0].Expression)
            {
                case IdentifierNameSyntax ins:
                    if (!context.SemanticModel.GetSymbolInfo(ins).Symbol.IsStatic)
                    {
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()));
                    }

                    return;
                case AnonymousMethodExpressionSyntax ame:
                    dfa = context.SemanticModel.AnalyzeDataFlow(ame);
                    break;
                case LambdaExpressionSyntax le:
                    dfa = context.SemanticModel.AnalyzeDataFlow(le);
                    break;
            }

            if (dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()));
            }
        }
	}
}
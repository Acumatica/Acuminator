using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Analyzers.Utilities;
using System.Collections.Generic;

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
            if (setDelegatememberAccessExpr?.Name.ToString() != "SetProcessDelegate") return;
            var setDelegateSymbol = context.SemanticModel.GetSymbolInfo(setDelegatememberAccessExpr).Symbol as IMethodSymbol;
            if (!setDelegateSymbol.ContainingType.ConstructedFrom.InheritsFromOrEquals(pxContext.PXProcessingBaseType))
                return;

            var ins = setDelegateInvocation.ArgumentList.Arguments[0].Expression as IdentifierNameSyntax;
            if (ins != null)
            {
                if(!context.SemanticModel.GetSymbolInfo(ins).Symbol.IsStatic)
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()));
                return;
            }

            DataFlowAnalysis dfa = null;
            var ame = setDelegateInvocation.ArgumentList.Arguments[0].Expression as AnonymousMethodExpressionSyntax;
            if (ame != null) dfa = context.SemanticModel.AnalyzeDataFlow(ame);
            var le = setDelegateInvocation.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
            if (le != null) dfa = context.SemanticModel.AnalyzeDataFlow(le);

            if (dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()));
        }
	}
}
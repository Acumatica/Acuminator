﻿using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
    {
		private const string SetProcessDelegateMethodName = "SetProcessDelegate";

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		public LongOperationDelegateClosuresAnalyzer() : this(null)
		{ }

		public LongOperationDelegateClosuresAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeSetProcessDelegateMethod(c, pxContext), SyntaxKind.InvocationExpression);
		}

		private static void AnalyzeSetProcessDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			var setDelegateInvocation = syntaxContext.Node as InvocationExpressionSyntax;

			if (!CheckIfDiagnosticIsValid(setDelegateInvocation, syntaxContext, pxContext))
				return;
            
            DataFlowAnalysis dfa = null;

            switch (setDelegateInvocation.ArgumentList.Arguments[0].Expression)
            {
                case IdentifierNameSyntax ins:
					ISymbol identifierSymbol = syntaxContext.SemanticModel.GetSymbolInfo(ins, syntaxContext.CancellationToken).Symbol;

					if (identifierSymbol != null && !identifierSymbol.IsStatic)
                    {
                        syntaxContext.ReportDiagnosticWithSuppressionCheck(
                            Diagnostic.Create(
                                Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()),
							pxContext.CodeAnalysisSettings);
                    }

                    return;
				case MemberAccessExpressionSyntax memberAccess 
				when memberAccess.Expression is ElementAccessExpressionSyntax arrayIndexAccess:
					AnalyzeMemberAccessExpressions(arrayIndexAccess.Expression, syntaxContext, pxContext);
					return;
				case MemberAccessExpressionSyntax memberAccess:
					AnalyzeMemberAccessExpressions(memberAccess.Expression, syntaxContext, pxContext);
					return;
				case ConditionalAccessExpressionSyntax conditionalAccess 
				when conditionalAccess.Expression is ElementAccessExpressionSyntax arrayIndexAccess:
					AnalyzeMemberAccessExpressions(arrayIndexAccess.Expression, syntaxContext, pxContext);
					return;
				case ConditionalAccessExpressionSyntax conditionalAccess:
					AnalyzeMemberAccessExpressions(conditionalAccess.Expression, syntaxContext, pxContext);
					return;
                case AnonymousMethodExpressionSyntax anonMethodNode:
                    dfa = syntaxContext.SemanticModel.AnalyzeDataFlow(anonMethodNode);
                    break;
                case LambdaExpressionSyntax lambdaNode:
                    dfa = syntaxContext.SemanticModel.AnalyzeDataFlow(lambdaNode);
                    break;
            }

            if (dfa != null && dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis))
            {
                syntaxContext.ReportDiagnosticWithSuppressionCheck(
                    Diagnostic.Create(
                        Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()),
					pxContext.CodeAnalysisSettings);
            }
        }

		private static bool CheckIfDiagnosticIsValid(InvocationExpressionSyntax setDelegateInvocation, SyntaxNodeAnalysisContext syntaxContext,
													 PXContext pxContext)
		{
			var setDelegatememberAccessExpr = setDelegateInvocation?.Expression as MemberAccessExpressionSyntax;

			if (setDelegatememberAccessExpr?.Name.ToString() != SetProcessDelegateMethodName ||
				syntaxContext.CancellationToken.IsCancellationRequested)
			{
				return false;
			}

			var setDelegateSymbol = syntaxContext.SemanticModel.GetSymbolInfo(setDelegatememberAccessExpr).Symbol as IMethodSymbol;

			if (setDelegateSymbol == null || !setDelegateSymbol.ContainingType.ConstructedFrom.InheritsFromOrEquals(pxContext.PXProcessingBase.Type))
				return false;

			return true;
		}

		private static void AnalyzeMemberAccessExpressions(ExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxContext, 
														   PXContext pxContext)
		{
			if (!(expression is IdentifierNameSyntax identifier))
				return;

			ISymbol identifierSymbol = syntaxContext.SemanticModel.GetSymbolInfo(identifier, syntaxContext.CancellationToken).Symbol;

			if (identifierSymbol == null || syntaxContext.CancellationToken.IsCancellationRequested)
				return;

			if ((identifierSymbol.Kind == SymbolKind.Field || identifierSymbol.Kind == SymbolKind.Property) && !identifierSymbol.IsStatic)
			{
				var setDelegateInvocation = syntaxContext.Node as InvocationExpressionSyntax;
				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1008_LongOperationDelegateClosures, setDelegateInvocation.GetLocation()),
					pxContext.CodeAnalysisSettings);
			}
		}
	}
}
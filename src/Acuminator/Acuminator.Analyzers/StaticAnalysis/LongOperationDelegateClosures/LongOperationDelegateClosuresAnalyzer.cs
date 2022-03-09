#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
    {
		private readonly LongOperationDelegateTypeClassifier _longOperationDelegateTypeClassifier = new LongOperationDelegateTypeClassifier();

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		public LongOperationDelegateClosuresAnalyzer() : this(null)
		{ }

		public LongOperationDelegateClosuresAnalyzer(CodeAnalysisSettings? codeAnalysisSettings) : base(codeAnalysisSettings)
		{ }

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
            compilationStartContext.RegisterSyntaxNodeAction(c => AnalyzeLongOperationDelegate(c, pxContext), SyntaxKind.InvocationExpression);
		}

		private void AnalyzeLongOperationDelegate(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
		{
			var longOperationSetupMethodInvocationNode = syntaxContext.Node as InvocationExpressionSyntax;
			var longOperationDelegateType = _longOperationDelegateTypeClassifier.GetLongOperationDelegateType(longOperationSetupMethodInvocationNode, 
																											  syntaxContext.SemanticModel, pxContext,
																											  syntaxContext.CancellationToken);
			switch (longOperationDelegateType)
			{
				case LongOperationDelegateType.ProcessingDelegate:
					AnalyzeSetProcessDelegateMethod(syntaxContext, pxContext, longOperationSetupMethodInvocationNode!);
					break;

				case LongOperationDelegateType.LongRunDelegate:
					break;
			}
		}

		private static void AnalyzeSetProcessDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, 
															InvocationExpressionSyntax setDelegateInvocation)
		{
			ExpressionSyntax processingDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[0].Expression;
			AnalyzeDataFlowForDelegateMethod(syntaxContext, pxContext, setDelegateInvocation, processingDelegateParameter);	

			if (setDelegateInvocation.ArgumentList.Arguments.Count > 1)
			{
				ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
				AnalyzeDataFlowForDelegateMethod(syntaxContext, pxContext, setDelegateInvocation, finallyHandlerDelegateParameter);
			}
		}

		private static void AnalyzeDataFlowForDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
															 InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
															 ExpressionSyntax longOperationDelegateNode)
		{
			switch (longOperationDelegateNode)
			{
				case IdentifierNameSyntax graphMemberName:
					ISymbol? graphMemberSymbol = syntaxContext.SemanticModel.GetSymbolInfo(graphMemberName, syntaxContext.CancellationToken).Symbol;

					if (graphMemberSymbol?.ContainingType != null && !graphMemberSymbol.IsStatic && 
						graphMemberSymbol.ContainingType.IsPXGraphOrExtension(pxContext))
					{
						syntaxContext.ReportDiagnosticWithSuppressionCheck(
							Diagnostic.Create(
								Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
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

				case AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode:
					DataFlowAnalysis? dfa = syntaxContext.SemanticModel.AnalyzeDataFlow(anonMethodOrLambdaNode);

					if (dfa != null && dfa.Succeeded && dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis))
					{
						syntaxContext.ReportDiagnosticWithSuppressionCheck(
							Diagnostic.Create(
								Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
								pxContext.CodeAnalysisSettings);
					}

					return;	
			}	
		}


		private static void AnalyzeMemberAccessExpressions(ExpressionSyntax expression, SyntaxNodeAnalysisContext syntaxContext, 
														   PXContext pxContext)
		{
			if (!(expression is IdentifierNameSyntax identifier))
				return;

			ISymbol identifierSymbol = syntaxContext.SemanticModel.GetSymbolInfo(identifier, syntaxContext.CancellationToken).Symbol;

			if (identifierSymbol == null || identifierSymbol.IsStatic)
				return;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			if (identifierSymbol.Kind == SymbolKind.Field || identifierSymbol.Kind == SymbolKind.Property)
			{
				var longOperationSetupMethodInvocationNode = (InvocationExpressionSyntax)syntaxContext.Node;

				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
						pxContext.CodeAnalysisSettings);
			}
		}
	}
}
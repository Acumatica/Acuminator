#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXGraphAggregatedAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1008_LongOperationDelegateClosures);

		public override bool ShouldAnalyze(PXContext pxContext, PXGraphSemanticModel graph) =>
			base.ShouldAnalyze(pxContext, graph) && graph.Type != GraphType.None &&
			!graph.Symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty;

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graph)
		{
			var longOperationsChecker = new LongOperationsChecker(context, pxContext, graph);

			foreach (SyntaxReference graphSyntaxReference in graph.Symbol.DeclaringSyntaxReferences)
			{
				var graphNode = graphSyntaxReference.GetSyntax(context.CancellationToken) as ClassDeclarationSyntax;

				if (graphNode != null)
					longOperationsChecker.CheckForCapturedGraphReferencesInDelegateClosures(graphNode);
			}			
		}


		//---------------------------------------Walker-------------------------------------------------------------------
		private class LongOperationsChecker : NestedInvocationWalker
		{
			private readonly SymbolAnalysisContext _context;
			private readonly PXGraphSemanticModel _graph;

			public LongOperationsChecker(SymbolAnalysisContext context, PXContext pxContext, PXGraphSemanticModel graph) : 
									base(pxContext, context.CancellationToken)
			{
				_context = context;
				_graph = graph;
			}

			public void CheckForCapturedGraphReferencesInDelegateClosures(ClassDeclarationSyntax graphNode)
			{
				ThrowIfCancellationRequested();

				graphNode.Accept(this);
			}

			public override void VisitMethodDeclaration(MethodDeclarationSyntax methodNode)
			{
				ThrowIfCancellationRequested();

				if (!methodNode.IsStatic())
					base.VisitMethodDeclaration(methodNode);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax longOperationSetupMethodInvocationNode) =>
				AnalyzeLongOperationDelegate(longOperationSetupMethodInvocationNode);

			private void AnalyzeLongOperationDelegate(InvocationExpressionSyntax longOperationSetupMethodInvocationNode)
			{
				ThrowIfCancellationRequested();

				SemanticModel? semanticModel = GetSemanticModel(longOperationSetupMethodInvocationNode.SyntaxTree);

				if (semanticModel == null)
				{
					base.VisitInvocationExpression(longOperationSetupMethodInvocationNode);
					return;
				}

				var longOperationDelegateType = LongOperationDelegateTypeClassifier.GetLongOperationDelegateType(longOperationSetupMethodInvocationNode,
																												 semanticModel, PxContext, CancellationToken);
				switch (longOperationDelegateType)
				{
					case LongOperationDelegateType.ProcessingDelegate:
						AnalyzeSetProcessDelegateMethod(semanticModel, longOperationSetupMethodInvocationNode);
						return;

					case LongOperationDelegateType.LongRunDelegate:
						AnalyzeStartOperationDelegateMethod(semanticModel, longOperationSetupMethodInvocationNode);
						return;

					default:
						base.VisitInvocationExpression(longOperationSetupMethodInvocationNode);
						return;
				}
			}

			private void AnalyzeSetProcessDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax setDelegateInvocation)
			{
				if (setDelegateInvocation.ArgumentList.Arguments.Count == 0)
					return;

				ExpressionSyntax processingDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[0].Expression;
				bool isMainProcessingDelegateCorrect = CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, processingDelegateParameter);

				if (isMainProcessingDelegateCorrect && setDelegateInvocation.ArgumentList.Arguments.Count > 1)
				{
					ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
					CheckDataFlowForDelegateMethod(semanticModel, setDelegateInvocation, finallyHandlerDelegateParameter);
				}
			}

			private void AnalyzeStartOperationDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax startOperationInvocation)
			{
				if (startOperationInvocation.ArgumentList.Arguments.Count < 2)
					return;

				ExpressionSyntax longRunDelegateParameter = startOperationInvocation.ArgumentList.Arguments[1].Expression;
				CheckDataFlowForDelegateMethod(semanticModel, startOperationInvocation, longRunDelegateParameter);
			}

			private bool CheckDataFlowForDelegateMethod(SemanticModel semanticModel, InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
														ExpressionSyntax longOperationDelegateNode)
			{
				ThrowIfCancellationRequested();

				var capturedLocalInstancesInExpressionsChecker =
					new CapturedLocalInstancesInExpressionsChecker(semanticModel, PxContext, CancellationToken);

				if (capturedLocalInstancesInExpressionsChecker.ExpressionCapturesLocalIntanceInClosure(longOperationDelegateNode))
				{
					_context.ReportDiagnosticWithSuppressionCheck(
						Diagnostic.Create(
							Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
							Settings);

					return false;
				}

				return true;
			}
		}
	}
}
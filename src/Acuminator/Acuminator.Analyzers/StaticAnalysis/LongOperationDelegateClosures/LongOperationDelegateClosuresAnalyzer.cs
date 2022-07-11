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
					AnalyzeStartOperationDelegateMethod(syntaxContext, pxContext, longOperationSetupMethodInvocationNode!);
					break;
			}
		}

		private static void AnalyzeSetProcessDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, 
															InvocationExpressionSyntax setDelegateInvocation)
		{
			if (setDelegateInvocation.ArgumentList.Arguments.Count == 0)
				return;

			ExpressionSyntax processingDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[0].Expression;
			bool isMainProcessingDelegateCorrect = CheckDataFlowForDelegateMethod(syntaxContext, pxContext, setDelegateInvocation, processingDelegateParameter);

			if (isMainProcessingDelegateCorrect && setDelegateInvocation.ArgumentList.Arguments.Count > 1)
			{
				ExpressionSyntax finallyHandlerDelegateParameter = setDelegateInvocation.ArgumentList.Arguments[1].Expression;
				CheckDataFlowForDelegateMethod(syntaxContext, pxContext, setDelegateInvocation, finallyHandlerDelegateParameter);
			}
		}

		private static void AnalyzeStartOperationDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
																InvocationExpressionSyntax startOperationInvocation)
		{
			if (startOperationInvocation.ArgumentList.Arguments.Count < 2)
				return;

			ExpressionSyntax longRunDelegateParameter = startOperationInvocation.ArgumentList.Arguments[1].Expression;
			CheckDataFlowForDelegateMethod(syntaxContext, pxContext, startOperationInvocation, longRunDelegateParameter);
		}

		private static bool CheckDataFlowForDelegateMethod(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
														   InvocationExpressionSyntax longOperationSetupMethodInvocationNode,
														   ExpressionSyntax longOperationDelegateNode)
		{
			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			var capturedLocalInstancesInExpressionsChecker = 
				new CapturedLocalInstancesInExpressionsChecker(syntaxContext.SemanticModel, pxContext, syntaxContext.CancellationToken);

			if (capturedLocalInstancesInExpressionsChecker.ExpressionCapturesLocalIntanceInClosure(longOperationDelegateNode))
			{
				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
						pxContext.CodeAnalysisSettings);

				return false;
			}

			return true;
		}
	}
}
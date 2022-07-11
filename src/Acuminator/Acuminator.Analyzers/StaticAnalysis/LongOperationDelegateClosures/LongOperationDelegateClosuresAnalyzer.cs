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

			if (DelegateNodeHoldsClosure(syntaxContext, pxContext, longOperationDelegateNode))
			{
				syntaxContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(
						Descriptors.PX1008_LongOperationDelegateClosures, longOperationSetupMethodInvocationNode.GetLocation()),
						pxContext.CodeAnalysisSettings);

				return false;
			}

			return true;
		}

		private static bool DelegateNodeHoldsClosure(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, ExpressionSyntax longOperationDelegateNode)
		{
			switch (longOperationDelegateNode)
			{
				case AnonymousFunctionExpressionSyntax anonMethodOrLambdaNode:

					DataFlowAnalysis? dfa = syntaxContext.SemanticModel.AnalyzeDataFlow(anonMethodOrLambdaNode);
					return dfa != null && dfa.Succeeded && dfa.DataFlowsIn.OfType<IParameterSymbol>().Any(p => p.IsThis);

				case IdentifierNameSyntax identifierName:
					return IdentifierHoldsClosure(syntaxContext, pxContext, identifierName);

				case MemberAccessExpressionSyntax memberAccess
				when memberAccess.Expression is ElementAccessExpressionSyntax arrayIndexAccess:
					return MemberAccessExpressionHoldsClosure(syntaxContext, pxContext, arrayIndexAccess.Expression);

				case MemberAccessExpressionSyntax memberAccess:
					return MemberAccessExpressionHoldsClosure(syntaxContext, pxContext, memberAccess.Expression);

				case ConditionalAccessExpressionSyntax conditionalAccess
				when conditionalAccess.Expression is ElementAccessExpressionSyntax arrayIndexAccess:
					return MemberAccessExpressionHoldsClosure(syntaxContext, pxContext, arrayIndexAccess.Expression);

				case ConditionalAccessExpressionSyntax conditionalAccess:
					return MemberAccessExpressionHoldsClosure(syntaxContext, pxContext, conditionalAccess.Expression);

				default:
					return false;
			}
		}

		private static bool MemberAccessExpressionHoldsClosure(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, ExpressionSyntax expression)
		{
			if (!(expression is IdentifierNameSyntax identifier))
				return false;

			syntaxContext.CancellationToken.ThrowIfCancellationRequested();

			return IdentifierHoldsClosure(syntaxContext, pxContext, identifier);
		}

		private static bool IdentifierHoldsClosure(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, IdentifierNameSyntax identifierName)
		{
			ISymbol? identifierSymbol = syntaxContext.SemanticModel.GetSymbolInfo(identifierName, syntaxContext.CancellationToken).Symbol;

			if (identifierSymbol?.ContainingType == null || identifierSymbol.IsStatic || !identifierSymbol.ContainingType.IsPXGraphOrExtension(pxContext))
				return false;

			switch (identifierSymbol.Kind)
			{
				case SymbolKind.Local:
					if (!(identifierSymbol is ILocalSymbol))
						return false;

					var localVariableDeclarator = identifierSymbol.DeclaringSyntaxReferences
																  .FirstOrDefault()
																 ?.GetSyntax(syntaxContext.CancellationToken) as VariableDeclaratorSyntax;
					if (localVariableDeclarator?.Initializer?.Value == null)
						return false;

					return DelegateNodeHoldsClosure(syntaxContext, pxContext, localVariableDeclarator.Initializer.Value);

				case SymbolKind.Method:
				case SymbolKind.Property:
				case SymbolKind.Event:
				case SymbolKind.Field:
					return true;             // Instance methods, properties, fields and events hold closure

				case SymbolKind.Parameter:    // We can't analyze parameter, so assume that they don't contains delegate with incorrect closures 
				default:
					return false;
			}
		}
	}
}
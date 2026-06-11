using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.IncorrectTaskUsageInAsyncCode
{
	public partial class IncorrectTaskUsageInAsyncCodeAnalyzer : PXDiagnosticAnalyzer
	{
		private class TaskUsageCheckingWalker : CSharpSyntaxWalker 
		{
			private readonly SyntaxNodeAnalysisContext _syntaxContext;
			private readonly PXContext _pxContext;

			private readonly INamedTypeSymbol? _valueTaskType;
			private readonly INamedTypeSymbol? _valueTaskGenericType;

			public CancellationToken Cancellation => _syntaxContext.CancellationToken;

			public SemanticModel SemanticModel => _syntaxContext.SemanticModel;

			public TaskUsageCheckingWalker(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
			{
				_syntaxContext 		  = syntaxContext;
				_pxContext	   		  = pxContext;
				_valueTaskType 		  = _pxContext.AsyncOperations.ValueTask;
				_valueTaskGenericType = _pxContext.AsyncOperations.ValueTask_Generic;
			}

			#region Visitor Optimization - do not visit some subtrees
			public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node) { }

			public override void VisitEnumDeclaration(EnumDeclarationSyntax node) { }

			public override void VisitDocumentationCommentTrivia(DocumentationCommentTriviaSyntax node) { }

			public override void VisitXmlComment(XmlCommentSyntax node) { }
			#endregion

			public override void VisitVariableDeclaration(VariableDeclarationSyntax variableDeclaration)
			{
				CheckVariableOrParameterTypeIsNotTask(variableDeclaration.Type);
				base.VisitVariableDeclaration(variableDeclaration);
			}

			public override void VisitParameter(ParameterSyntax parameter)
			{
				CheckVariableOrParameterTypeIsNotTask(parameter.Type);
				base.VisitParameter(parameter);
			}

			private void CheckVariableOrParameterTypeIsNotTask(TypeSyntax? typeNode)
			{
				Cancellation.ThrowIfCancellationRequested();

				if (typeNode == null)
					return;

				var variableType = SemanticModel.GetSymbolOrBestCandidate(typeNode, Cancellation) as ITypeSymbol;

				if (variableType == null || !IsTaskType(variableType))
					return;

				var location   = typeNode.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_StoreTaskInVariable, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression)
			{
				Cancellation.ThrowIfCancellationRequested();

				// Do a cheaper check for awaited or immediately returned invocations first to avoid extra queries to the semantic model
				if (invocationExpression.Parent is AwaitExpressionSyntax or ReturnStatementSyntax or ArrowExpressionClauseSyntax or
												   AnonymousFunctionExpressionSyntax)
				{
					base.VisitInvocationExpression(invocationExpression);
					return;
				}

				var invocationType = SemanticModel.GetTypeInfo(invocationExpression, Cancellation).Type;

				if (invocationType == null || invocationType.SpecialType != SpecialType.None || !IsTaskType(invocationType))
				{
					base.VisitInvocationExpression(invocationExpression);
					return;
				}

				var location = invocationExpression.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_NotAwaitedTaskReturningExpression, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
				base.VisitInvocationExpression(invocationExpression);
			}

			public override void VisitReturnStatement(ReturnStatementSyntax returnStatement)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(returnStatement.Expression);
				base.VisitReturnStatement(returnStatement);
			}

			public override void VisitArrowExpressionClause(ArrowExpressionClauseSyntax arrowExpressionMethodBody)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(arrowExpressionMethodBody.Expression);
				base.VisitArrowExpressionClause(arrowExpressionMethodBody);
			}

			// No need to check anonymous methods in addition to lambdas - they are covered by visiting return expressions

			public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax parenthesizedLambdaExpression)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(parenthesizedLambdaExpression.ExpressionBody);
				base.VisitParenthesizedLambdaExpression(parenthesizedLambdaExpression);
			}

			public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax simpleLambdaExpression)
			{
				Cancellation.ThrowIfCancellationRequested();
				CheckTypeMemberReturningTaskHasTaskReturnType(simpleLambdaExpression.ExpressionBody);
				base.VisitSimpleLambdaExpression(simpleLambdaExpression);
			}

			private void CheckTypeMemberReturningTaskHasTaskReturnType(ExpressionSyntax? returnExpression)
			{
				if (returnExpression == null)
					return;

				var returnExpressionType = SemanticModel.GetTypeInfo(returnExpression, Cancellation).Type;

				if (returnExpressionType == null || !IsTaskType(returnExpressionType))
					return;

				var containingMethodOrLocalFunction = returnExpression.GetContainingMemberOrLocalFunctionOrLambda();
				var returnTypeSymbol = GetContainingMethodReturnType(containingMethodOrLocalFunction);

				if (returnTypeSymbol == null || IsTaskType(returnTypeSymbol))
					return;

				var location   = returnExpression.GetLocation();
				var diagnostic = Diagnostic.Create(Descriptors.PX1120_IncorrectTaskUsageInAsyncCode_MethodReturnTypeIsNotTask, location);

				_syntaxContext.ReportDiagnosticWithSuppressionCheck(diagnostic, _pxContext.CodeAnalysisSettings);
			}

			private ITypeSymbol? GetContainingMethodReturnType(SyntaxNode? containingMethodOrLocalFunction)
			{
				if (containingMethodOrLocalFunction is AnonymousFunctionExpressionSyntax lambdaDeclaration)
				{
					var lambdaSymbol = SemanticModel.GetSymbolOrBestCandidate(lambdaDeclaration, Cancellation) as IMethodSymbol;
					return lambdaSymbol?.ReturnType;
				}

				var returnTypeNode = containingMethodOrLocalFunction switch
				{
					MethodDeclarationSyntax methodDeclaration	  => methodDeclaration.ReturnType,
					PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Type,
					LocalFunctionStatementSyntax localFunction	  => localFunction.ReturnType,
					_ 											  => null
				};

				if (returnTypeNode == null)
					return null;

				var returnTypeSymbol = SemanticModel.GetSymbolOrBestCandidate(returnTypeNode, Cancellation) as ITypeSymbol;
				return returnTypeSymbol;
			}

			private bool IsTaskType(ITypeSymbol typeSymbol) =>
				typeSymbol.Equals(_pxContext.AsyncOperations.Task, SymbolEqualityComparer.Default) ||
				typeSymbol.OriginalDefinition.Equals(_pxContext.AsyncOperations.Task_Generic, SymbolEqualityComparer.Default) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol, _valueTaskType) ||
				SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, _valueTaskGenericType);
		}
	}
}
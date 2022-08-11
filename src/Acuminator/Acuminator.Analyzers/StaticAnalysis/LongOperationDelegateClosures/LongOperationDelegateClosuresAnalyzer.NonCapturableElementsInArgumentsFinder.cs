#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures
{
	public partial class LongOperationDelegateClosuresAnalyzer : PXDiagnosticAnalyzer
	{
		private class NonCapturableElementsInArgumentsFinder : CSharpSyntaxVisitor
		{
			private readonly CancellationToken _cancellation;
			private readonly PXContext _pxContext;
	
			private IReadOnlyDictionary<string, PassedParameter>? _parametersToCheck;
			private ISymbol? _callingTypeMember;

			private List<PassedParameter>? _callingMethodParametersUsedInArgument;
			private bool _captureLocalGraphInstance;

			public NonCapturableElementsInArgumentsFinder(CancellationToken cancellation, PXContext pxContext)
			{
				_cancellation = cancellation;
				_pxContext = pxContext;
			}

			public (bool CaptureLocalGraphInstance, List<PassedParameter>? ParametersUsedInArgument) GetElementsUsedInArgumentExpression(
																							 ArgumentSyntax argument, ISymbol callingTypeMember,
																							 IReadOnlyDictionary<string, PassedParameter>? parametersToCheck)
			{
				_cancellation.ThrowIfCancellationRequested();

				if (argument.Expression == null)
					return default;

				try
				{
					InitializeState(callingTypeMember, parametersToCheck);
					argument.Expression.Accept(this);

					var listWithoutDuplicates = _callingMethodParametersUsedInArgument?.Distinct()
																					   .ToList(_callingMethodParametersUsedInArgument.Count);
					return (_captureLocalGraphInstance, listWithoutDuplicates);
				}
				finally
				{
					// We do not clear the collected _callingMethodParametersUsedInArgument collection instance here, just reset the stored reference to it to null
					// This does not affect the returned reference to _callingMethodParametersUsedInArgument in the try block
					// since for reference types C# returns a copy of a reference to an object by default.
					// 
					// By default C# returns by value and a ref modifier is required to return the reference to the field itself.
					ClearState();
				}
			}

			public override void DefaultVisit(SyntaxNode node)
			{
				if (node is not ExpressionSyntax expression)
					return;

				foreach (SyntaxNode childNode in expression.ChildNodes())
				{
					if (childNode is ExpressionSyntax childExpression)			
					{
						childExpression.Accept(this);
						continue;
					}

					var innerExpression = childNode.DescendantNodes().OfType<ExpressionSyntax>().FirstOrDefault();
					innerExpression?.Accept(this);
				}
			}

			public override void VisitIdentifierName(IdentifierNameSyntax identifier)
			{
				if (_parametersToCheck?.Count > 0)
				{
					string identifierName = identifier.Identifier.ValueText;

					if (_parametersToCheck.TryGetValue(identifierName, out var passedParameter))
						AddToUsedParameters(passedParameter);
				}
			}

			public override void VisitElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpression)
			{
				if (_parametersToCheck?.Count > 0)
				{
					if (elementAccessExpression.Expression is IdentifierNameSyntax elementAccessIdentifier)
						VisitIdentifierName(elementAccessIdentifier);
					else
					{
						elementAccessExpression.Expression?.Accept(this);
					}
				}
			}

			public override void VisitThisExpression(ThisExpressionSyntax thisExpression)
			{
				if (_captureLocalGraphInstance)
					return;

				if (_callingTypeMember!.ContainingType.IsPXGraphOrExtension(_pxContext) && !_callingTypeMember.IsStatic)
				{
					_captureLocalGraphInstance = true;
				}
			}

			public override void VisitInitializerExpression(InitializerExpressionSyntax initializer)
			{
				foreach (ExpressionSyntax expr in initializer.Expressions)
					expr.Accept(this);
			}

			public override void VisitTupleExpression(TupleExpressionSyntax tupleExpression)
			{
				foreach (ArgumentSyntax argument in tupleExpression.Arguments)
					argument.Accept(this);
			}

			public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentExpression) =>
				assignmentExpression.Right?.Accept(this);

			public override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax parenthesizedExpression) =>
				parenthesizedExpression.Expression?.Accept(this);

			public override void VisitConditionalExpression(ConditionalExpressionSyntax conditionalExpression)
			{
				conditionalExpression.WhenTrue?.Accept(this);
				conditionalExpression.WhenFalse?.Accept(this);
			}

			public override void VisitBinaryExpression(BinaryExpressionSyntax binaryExpression)
			{
				if (binaryExpression.IsKind(SyntaxKind.CoalesceExpression))
				{
					binaryExpression.Left?.Accept(this);
					binaryExpression.Right?.Accept(this);
				}
			}

			#region Filtered Subtrees
			public override void VisitInvocationExpression(InvocationExpressionSyntax invocationExpression) { }

			public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression) { }

			public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccessExpression) { }

			public override void VisitPostfixUnaryExpression(PostfixUnaryExpressionSyntax postfixUnaryExpression) { }

			public override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax prefixUnaryExpression) { }

			public override void VisitTypeOfExpression(TypeOfExpressionSyntax typeOfExpression) { }

			public override void VisitLiteralExpression(LiteralExpressionSyntax node) { }
			#endregion

			#region State Management
			private void InitializeState(ISymbol callingTypeMember, IReadOnlyDictionary<string, PassedParameter>? parametersToCheck)
			{
				_callingTypeMember = callingTypeMember;
				_parametersToCheck = parametersToCheck;
			}

			private void ClearState()
			{
				_parametersToCheck = null;
				_callingTypeMember = null;
				_callingMethodParametersUsedInArgument = null;
				_captureLocalGraphInstance = false;
			}

			private void AddToUsedParameters(PassedParameter parameter)
			{
				_callingMethodParametersUsedInArgument = _callingMethodParametersUsedInArgument ?? new List<PassedParameter>();
				_callingMethodParametersUsedInArgument.Add(parameter);
			}
			#endregion
		}
	}
}
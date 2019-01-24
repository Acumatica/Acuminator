using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.BqlParameterMismatch
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		protected class ArrayElemCountLocalVariableResolver : BqlInvocationDataFlowAnalyserBase
		{
			private readonly ResolveArrayElemCountMethodBodyWalker _methodBodyWalker;

			public ArrayElemCountLocalVariableResolver(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext,
													   IdentifierNameSyntax identifierNode) :
												  base(syntaxContext, pxContext, identifierNode)
			{
				_methodBodyWalker = new ResolveArrayElemCountMethodBodyWalker(this);
			}

			/// <summary>
			/// Gets array elements count in array variable. If <c>null</c> then the number couldn't be counted.
			/// </summary>
			/// <returns/>
			public int? GetArrayElementsCount()
			{
				if (CancellationToken.IsCancellationRequested)
					return null;

				MethodDeclarationSyntax methodDeclaration = Invocation.GetDeclaringMethodNode();

				if (methodDeclaration == null || !SemanticModel.IsLocalVariable(methodDeclaration, VariableName))
					return null;

				_methodBodyWalker.Visit(methodDeclaration);

				if (CancellationToken.IsCancellationRequested || !_methodBodyWalker.IsValid || _methodBodyWalker.Candidates.Count == 0)
					return null;

				return GetElementsCountFromCandidates();
			}

			private int? GetElementsCountFromCandidates()
			{
				while (_methodBodyWalker.Candidates.Count > 0)
				{
					var (potentialAssignmentStatement, count) = _methodBodyWalker.Candidates.Pop();
					var (analysisSucceded, varAlwaysAssigned) = CheckCandidate(potentialAssignmentStatement);

					if (!analysisSucceded || !varAlwaysAssigned || count == null)
						return null;    //analysis failed or reacheable assignment with not always assigned variable or valid candidate with unresolvable count

					return count;
				}

				return null;
			}



			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			private class ResolveArrayElemCountMethodBodyWalker : CSharpSyntaxWalker
			{
				private readonly ArrayElemCountLocalVariableResolver _resolver;

				private bool _shouldStop;
				private bool _isValid = true;
				private bool IsCancelationRequested => _resolver.CancellationToken.IsCancellationRequested;

				public Stack<(StatementSyntax PotentialAssignment, int? ElementsCount)> Candidates { get; }

				public bool IsValid
				{
					get => _isValid;
					set
					{
						if (value == false)
						{
							_isValid = false;
							_shouldStop = true;
						}
					}
				}

				public ResolveArrayElemCountMethodBodyWalker(ArrayElemCountLocalVariableResolver aResolver)
				{
					_resolver = aResolver;
					Candidates = new Stack<(StatementSyntax, int?)>(capacity: 2);
				}

				public override void Visit(SyntaxNode node)
				{
					if (IsCancelationRequested)
					{
						IsValid = false;
					}

					if (_shouldStop)
						return;

					base.Visit(node);
				}

				public override void VisitVariableDeclarator(VariableDeclaratorSyntax declarator)
				{
					if (IsCancelationRequested || declarator.Identifier.ValueText != _resolver.VariableName ||
						declarator.Initializer?.Value == null)
					{
						if (!IsCancelationRequested)
							base.VisitVariableDeclarator(declarator);

						return;
					}

					var declaratorStatement = declarator.GetStatementNode();

					if (!_resolver.IsReacheableByControlFlow(declaratorStatement))
						return;

					int? countOfArrayArgs = RoslynSyntaxUtils.TryGetSizeOfSingleDimensionalNonJaggedArray(declarator.Initializer.Value,
																										  _resolver.SemanticModel,
																										  _resolver.CancellationToken);
					Candidates.Push((declaratorStatement, countOfArrayArgs));
				}

				public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignment)
				{
					if (IsCancelationRequested)
						return;

					ExpressionSyntax curExpression = assignment;
					AssignmentExpressionSyntax candidateAssignment = null;

					while (curExpression is AssignmentExpressionSyntax curAssignment)
					{
						if (candidateAssignment == null && curAssignment.Left is IdentifierNameSyntax identifier &&
							identifier.Identifier.ValueText == _resolver.VariableName)
						{
							candidateAssignment = curAssignment;
						}

						curExpression = curAssignment.Right;
					}

					if (candidateAssignment == null || IsCancelationRequested)
						return;

					var assignmentStatement = candidateAssignment.GetStatementNode();

					if (!_resolver.IsReacheableByControlFlow(assignmentStatement))
						return;

					int? countOfArrayArgs = RoslynSyntaxUtils.TryGetSizeOfSingleDimensionalNonJaggedArray(curExpression, _resolver.SemanticModel,
																										  _resolver.CancellationToken);
					Candidates.Push((assignmentStatement, countOfArrayArgs));
				}

				public override void VisitGotoStatement(GotoStatementSyntax node)
				{
					if (node.IsKind(SyntaxKind.GotoStatement))   //The analysis is not valid if there are goto statements in method. This is rarely a case in C#
					{
						IsValid = false;
					}
				}

				public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
				{
					if (_shouldStop)
						return;

					if (invocation.Equals(_resolver.Invocation))
					{
						_shouldStop = true;
						return;
					}

					if (!AnalyzeInvocation(invocation))
					{
						IsValid = false;
						return;
					}

					if (IsCancelationRequested)
						return;

					base.VisitInvocationExpression(invocation);
				}

				/// <summary>
				/// Analyze invocation. Returns <c>true</c> if invocation is valid for diagnostic, <c>false</c> if diagnostic can't be made with given invocation inside method.
				/// </summary>
				/// <param name="invocation">The invocation node.</param>
				/// <returns/>
				private bool AnalyzeInvocation(InvocationExpressionSyntax invocation)
				{
					bool notPassiingVariableByRef = invocation.GetArgumentsContainingIdentifier(_resolver.VariableName)
															  .All(arg => arg.RefOrOutKeyword.IsKind(SyntaxKind.None));

					if (notPassiingVariableByRef)   //Check only for method calls passing variable by ref because they could modify it in the method body
						return true;

					try
					{
						ControlFlowAnalysis controlFlow = _resolver.SemanticModel.AnalyzeControlFlow(invocation.GetStatementNode());

						if (controlFlow?.Succeeded == true && !controlFlow.EndPointIsReachable)
							return true;
					}
					catch (Exception e)
					{
						return false;
					}

					return false;
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
		protected class BqlLocalVariableTypeResolver : BqlInvocationDataFlowAnalyserBase
		{
			private readonly ResolveVarTypeMethodBodyWalker _methodBodyWalker;

			public BqlLocalVariableTypeResolver(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, 
												InvocationExpressionSyntax invocation, IdentifierNameSyntax identifierNode) :
										   base(syntaxContext, invocation, pxContext, identifierNode)
			{
				_methodBodyWalker = new ResolveVarTypeMethodBodyWalker(this);
			}

			public bool CheckForBqlModifications()
			{
				if (CancellationToken.IsCancellationRequested)
					return false;

				MethodDeclarationSyntax methodDeclaration = Invocation.GetDeclaringMethodNode();

				if (methodDeclaration == null || CancellationToken.IsCancellationRequested)
					return false;

				_methodBodyWalker.Visit(methodDeclaration);
				return _methodBodyWalker.IsValid && !CancellationToken.IsCancellationRequested;
			}

			public ITypeSymbol ResolveVariableType()
			{
				if (CancellationToken.IsCancellationRequested)
					return null;

				MethodDeclarationSyntax methodDeclaration = Invocation.GetDeclaringMethodNode();

				if (methodDeclaration == null || !SemanticModel.IsLocalVariable(methodDeclaration, VariableName))
					return null;

				_methodBodyWalker.Visit(methodDeclaration);

				if (CancellationToken.IsCancellationRequested || !_methodBodyWalker.IsValid || _methodBodyWalker.Candidates.Count == 0)
					return null;

				TypeSyntax assignedType = GetTypeFromCandidates();

				if (assignedType == null)
					return null;

				SymbolInfo symbolInfo = SemanticModel.GetSymbolInfo(assignedType);
				return symbolInfo.Symbol as ITypeSymbol;
			}

			private TypeSyntax GetTypeFromCandidates()
			{
				while (_methodBodyWalker.Candidates.Count > 0)
				{
					var (potentialAssignmentStatement, assignedType) = _methodBodyWalker.Candidates.Pop();
					var (analysisSucceded, varAlwaysAssigned) = CheckCandidate(potentialAssignmentStatement);

					if (!analysisSucceded || !varAlwaysAssigned || assignedType == null)
						return null;    //analysis failed or reacheable assignment with not always assigned variable or valid candidate with unresolvable type

					return assignedType;
				}

				return null;
			}




			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			private class ResolveVarTypeMethodBodyWalker : CSharpSyntaxWalker
			{
				private readonly BqlLocalVariableTypeResolver resolver;

				private bool _isInAnalysedVariableInvocation;
				private bool _shouldStop;
				private bool _isValid = true;

				private bool IsCancelationRequested => resolver.CancellationToken.IsCancellationRequested;

				public Stack<(StatementSyntax PotentialAssignment, TypeSyntax AssignedType)> Candidates { get; }

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

				public ResolveVarTypeMethodBodyWalker(BqlLocalVariableTypeResolver aResolver)
				{
					resolver = aResolver;
					Candidates = new Stack<(StatementSyntax, TypeSyntax)>(capacity: 2);
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
					if (IsCancelationRequested || declarator.Identifier.ValueText != resolver.VariableName ||
						declarator.Initializer?.Value == null)
					{
						if (!IsCancelationRequested)
							base.VisitVariableDeclarator(declarator);

						return;
					}

					var declaratorStatement = declarator.GetStatementNode();

					if (!resolver.IsReacheableByControlFlow(declaratorStatement))
						return;

					switch (declarator.Initializer.Value)
					{
						case ObjectCreationExpressionSyntax objectCreation:
							Candidates.Push((declaratorStatement, AssignedType: objectCreation.Type));
							return;
						default:
							Candidates.Push((declaratorStatement, AssignedType: null));
							return;
					}
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
							identifier.Identifier.ValueText == resolver.VariableName)
						{
							candidateAssignment = curAssignment;
						}

						curExpression = curAssignment.Right;
					}

					if (candidateAssignment == null || IsCancelationRequested)
						return;

					var assignmentStatement = candidateAssignment.GetStatementNode();

					if (!resolver.IsReacheableByControlFlow(assignmentStatement))
						return;

					switch (curExpression)
					{
						case ObjectCreationExpressionSyntax objectCreation:
							Candidates.Push((assignmentStatement, AssignedType: objectCreation.Type));
							return;
						default:
							Candidates.Push((assignmentStatement, AssignedType: null));
							return;
					}
				}

				public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
				{
					if (IsCancelationRequested)
						return;

					if (_isInAnalysedVariableInvocation || !(conditionalAccess.Expression is IdentifierNameSyntax identifier) ||
						identifier.Identifier.ValueText != resolver.VariableName)
					{
						base.VisitConditionalAccessExpression(conditionalAccess);
					}

					try
					{
						_isInAnalysedVariableInvocation = true;
						base.VisitConditionalAccessExpression(conditionalAccess);
					}
					finally
					{
						_isInAnalysedVariableInvocation = false;
					}
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

					if (invocation.Equals(resolver.Invocation))
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
					if (invocation.DoesArgumentsContainIdentifier(resolver.VariableName))   //Check for all method calls which take bql as parameter because they could modify it in the method body
						return false;

					if (!IsInvocationOnAnalysedVariable(invocation))
						return true;

					var symbolInfo = resolver.SemanticModel.GetSymbolInfo(invocation);

					if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
						return true;

					if (methodSymbol.IsStatic || !BqlModifyingMethods.IsBqlModifyingInstanceMethod(methodSymbol, resolver.PXContext))
						return true;

					try
					{
						ControlFlowAnalysis controlFlow = resolver.SemanticModel.AnalyzeControlFlow(invocation.GetStatementNode());

						if (controlFlow?.Succeeded == true && !controlFlow.EndPointIsReachable)
							return true;
					}
					catch
					{
						return false;
					}

					return false;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private bool IsInvocationOnAnalysedVariable(InvocationExpressionSyntax invocation)
				{
					if (_isInAnalysedVariableInvocation)
						return true;

					return invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
						   memberAccess.OperatorToken.IsKind(SyntaxKind.DotToken) &&
						   memberAccess.Expression is IdentifierNameSyntax identifier &&
						   identifier.Identifier.ValueText == resolver.VariableName;
				}
			}
		}
	}
}

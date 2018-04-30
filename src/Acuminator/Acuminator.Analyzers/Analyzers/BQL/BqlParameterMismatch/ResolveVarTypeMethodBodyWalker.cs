using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities;
using PX.Data;


namespace Acuminator.Analyzers
{
	public partial class BqlParameterMismatchAnalyzer : PXDiagnosticAnalyzer
	{
		protected class LocalVariableTypeResolver
		{
			private readonly ResolveVarTypeMethodBodyWalker methodBodyWalker;

			private readonly SyntaxNodeAnalysisContext syntaxContext;
			private readonly PXContext pxContext;		
			private readonly IdentifierNameSyntax identifier;
			
			private InvocationExpressionSyntax Invocation { get; }

			private string VariableName { get; }

			private SemanticModel SemanticModel => syntaxContext.SemanticModel;

			private CancellationToken CancellationToken => syntaxContext.CancellationToken;

			public LocalVariableTypeResolver(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext, IdentifierNameSyntax identifierNode)
			{
				syntaxContext = aSyntaxContext;
				pxContext = aPxContext;
				identifier = identifierNode;
				Invocation = syntaxContext.Node as InvocationExpressionSyntax;		
				VariableName = identifierNode.Identifier.ValueText;
				methodBodyWalker = new ResolveVarTypeMethodBodyWalker(this);
			}

			public ITypeSymbol ResolveVariableType()
			{
				if (CancellationToken.IsCancellationRequested)
					return null;

				MethodDeclarationSyntax methodDeclaration = Invocation.GetDeclaringMethodNode();

				if (methodDeclaration == null)
					return null;

				if (!IsLocalVariable(methodDeclaration))
					return null;

				methodBodyWalker.Visit(methodDeclaration);

				if (CancellationToken.IsCancellationRequested || !methodBodyWalker.IsValid || methodBodyWalker.Candidates.Count != 1)
					return null;

				var (potentialAssignment, assignedType) = methodBodyWalker.Candidates[0];
				TypeInfo typeInfo = SemanticModel.GetTypeInfo(assignedType);
				return typeInfo.Type;
			}

			

			private bool IsLocalVariable(MethodDeclarationSyntax containingMethod)
			{
				DataFlowAnalysis dataFlowAnalysis = SemanticModel.AnalyzeDataFlow(containingMethod.Body);

				if (dataFlowAnalysis == null || !dataFlowAnalysis.Succeeded)
					return false;

				return dataFlowAnalysis.VariablesDeclared.Any(var => var.Name == VariableName);
			}




			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			//*****************************************************************************************************************************************************************************
			private class ResolveVarTypeMethodBodyWalker : CSharpSyntaxWalker
			{
				private readonly LocalVariableTypeResolver resolver;

				private bool isInAnalysedVariableInvocation;
				private bool shouldStop;
				private bool isValid = true;

				private bool IsCancelationRequested => resolver.CancellationToken.IsCancellationRequested;

				public List<(SyntaxNode PotentialAssignment, TypeSyntax AssignedType)> Candidates { get; }
				
				public bool IsValid
				{
					get => isValid;
					set
					{
						if (value == false)
						{
							isValid = false;
							shouldStop = true;
						}
					}
				}

				public ResolveVarTypeMethodBodyWalker(LocalVariableTypeResolver aResolver)
				{
					resolver = aResolver;
					Candidates = new List<(SyntaxNode, TypeSyntax)>(capacity: 2);
				}

				public override void Visit(SyntaxNode node)
				{
					if (IsCancelationRequested)
					{
						IsValid = false;
					}

					if (shouldStop)
						return;

					base.Visit(node);
				}

				public override void VisitVariableDeclarator(VariableDeclaratorSyntax declarator)
				{
					if (IsCancelationRequested || declarator.Identifier.ValueText != resolver.VariableName ||
					   !(declarator.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation) || !IsReacheable(declarator))
					{
						if (!IsCancelationRequested)
							base.VisitVariableDeclarator(declarator);

						return;
					}

					Candidates.Add((declarator, objectCreation.Type));

					if (!IsCancelationRequested)
						base.VisitVariableDeclarator(declarator);
				}
				
				public override void VisitAssignmentExpression(AssignmentExpressionSyntax assignment)
				{
					if (IsCancelationRequested || !IsReacheable(assignment))
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
					{
						return;
					}

					if (!(curExpression is ObjectCreationExpressionSyntax objectCreation))
					{
						IsValid = false;
						return;
					}

					Candidates.Add((candidateAssignment, objectCreation.Type));

					if (!IsCancelationRequested)
						base.VisitAssignmentExpression(assignment);
				}

				public override void VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax conditionalAccess)
				{
					if (IsCancelationRequested)
						return;
							
					if (isInAnalysedVariableInvocation || !(conditionalAccess.Expression is IdentifierNameSyntax identifier) ||
						identifier.Identifier.ValueText != resolver.VariableName)
					{
						base.VisitConditionalAccessExpression(conditionalAccess);
					}

					try
					{
						isInAnalysedVariableInvocation = true;
						base.VisitConditionalAccessExpression(conditionalAccess);
					}
					finally
					{
						isInAnalysedVariableInvocation = false;
					}			
				}

				public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
				{
					if (shouldStop)
						return;

					if (invocation.Equals(resolver.Invocation))
					{
						shouldStop = true;
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
					if (invocation.ArgumentsContainIdentifier(resolver.VariableName))   //Check for all method calls which take bql as parameter because they could modify it in the method body
						return false;

					if (!IsInvocationOnAnalysedVariable(invocation))
						return true;

					var symbolInfo = resolver.SemanticModel.GetSymbolInfo(invocation);

					if (!(symbolInfo.Symbol is IMethodSymbol methodSymbol))
						return true;

					if (methodSymbol.IsStatic || !BqlModifyingMethods.IsBqlModifyingInstanceMethod(methodSymbol, resolver.pxContext))
						return true;

					var invocationStatement = invocation.GetStatementNode();

					return invocationStatement != null
						? IsReacheable(invocationStatement) 
						: false;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private bool IsInvocationOnAnalysedVariable(InvocationExpressionSyntax invocation)
				{
					if (isInAnalysedVariableInvocation)
						return true;

					return invocation.Expression is MemberAccessExpressionSyntax memberAccess && 
						   memberAccess.OperatorToken.IsKind(SyntaxKind.DotToken) &&
						   memberAccess.Expression is IdentifierNameSyntax identifier && 
						   identifier.Identifier.ValueText == resolver.VariableName;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private bool IsReacheable(SyntaxNode assignmentNode)
				{
					StatementSyntax assignmentStatement = (assignmentNode as StatementSyntax) ?? assignmentNode.GetStatementNode();
					StatementSyntax invocationStatement = resolver.Invocation.GetStatementNode();

					(bool isReacheable, StatementSyntax scopedInvocationStatement) = 
						AnalyzeControlFlowBetween(invocationStatement, assignmentStatement);

					if (!isReacheable)
						return false;

					StatementSyntax nextStatementAfterAssignment = assignmentStatement.GetNextStatement();

					if (nextStatementAfterAssignment == null || nextStatementAfterAssignment.Equals(invocationStatement))
						return nextStatementAfterAssignment != null;

					DataFlowAnalysis flowAnalysis = null;

					try
					{
						flowAnalysis = resolver.SemanticModel.AnalyzeDataFlow(nextStatementAfterAssignment, scopedInvocationStatement);		
					}
					catch (Exception e)
					{
						return true;    //If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable 
					}

					if (flowAnalysis?.Succeeded != true)
						return false;

					if (flowAnalysis.AlwaysAssigned.All(var => var.Name != resolver.VariableName))
						return false;

					
					return true;
				}

				private (bool IsReacheable, StatementSyntax ScopedInvocationStatement) AnalyzeControlFlowBetween(StatementSyntax invocationStatement, 
																												 StatementSyntax assignmentStatement)
				{
					SyntaxNode assignmentScope = assignmentStatement.Parent;
					SyntaxNode currentInvocationScope = invocationStatement.Parent;

					if (assignmentScope.Equals(currentInvocationScope))
					{
						return (true, invocationStatement);
					}

					SyntaxNode prevNode = invocationStatement;

					while (!(currentInvocationScope is MethodDeclarationSyntax))
					{
						if (currentInvocationScope.Equals(assignmentScope))
						{
							StatementSyntax scopedStatement = (prevNode as StatementSyntax) ?? prevNode.GetStatementNode();
							return (scopedStatement != null, scopedStatement);
						}


						prevNode = currentInvocationScope;
						currentInvocationScope = currentInvocationScope.Parent;
					}

					return (false, null);
				}

				
			}
		}	
	}
}

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
            private readonly StatementSyntax invocationStatement;

            private InvocationExpressionSyntax Invocation { get; }

			private string VariableName { get; }

			private SemanticModel SemanticModel => syntaxContext.SemanticModel;

			private CancellationToken CancellationToken => syntaxContext.CancellationToken;

			public LocalVariableTypeResolver(SyntaxNodeAnalysisContext aSyntaxContext, PXContext aPxContext, IdentifierNameSyntax identifierNode)
			{
				syntaxContext = aSyntaxContext;
				pxContext = aPxContext;
				Invocation = syntaxContext.Node as InvocationExpressionSyntax;
                invocationStatement = Invocation.GetStatementNode();
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

				if (CancellationToken.IsCancellationRequested || !methodBodyWalker.IsValid || methodBodyWalker.Candidates.Count == 0)
					return null;

                List<TypeSyntax> checkedCandidates = AnalyzeCandidates();

                if (checkedCandidates.Count != 1)
                    return null;

                TypeSyntax assignedType = checkedCandidates[0];
				SymbolInfo symbolInfo = SemanticModel.GetSymbolInfo(assignedType);
				return symbolInfo.Symbol as ITypeSymbol;
			}

			private bool IsLocalVariable(MethodDeclarationSyntax containingMethod)
			{
				DataFlowAnalysis dataFlowAnalysis = SemanticModel.AnalyzeDataFlow(containingMethod.Body);

				if (dataFlowAnalysis == null || !dataFlowAnalysis.Succeeded)
					return false;

				return dataFlowAnalysis.VariablesDeclared.Any(var => var.Name == VariableName);
			}

            private List<TypeSyntax> AnalyzeCandidates()
            {
                List<TypeSyntax> checkedCandidates = new List<TypeSyntax>(methodBodyWalker.Candidates.Count);

                while (methodBodyWalker.Candidates.Count > 0)
                {
                    var (potentialAssignmentStatement, assignedType) = methodBodyWalker.Candidates.Pop();

                    if (IsReacheable(potentialAssignmentStatement, checkedCandidates.Count))
                        checkedCandidates.Add(assignedType);
                }

                return checkedCandidates;
            }

            private bool IsReacheable(StatementSyntax assignmentStatement, int countOfReachableCandidatesAfter)
            {
                (bool isSuccess, bool isReacheable, StatementSyntax scopedAssignment, StatementSyntax scopedInvocation) =
                    AnalyzeControlFlowBetweenAssignmentAndInvocation(invocationStatement, assignmentStatement);

                if (!isSuccess)
                    return true;   //If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable 
                else if (!isReacheable)
                    return false;

                StatementSyntax nextStatementAfterAssignment = scopedAssignment.GetNextStatement();

                if (nextStatementAfterAssignment == null)
                    return false;

                DataFlowAnalysis flowAnalysisWithoutAssignment = null;

                try
                {
                    flowAnalysisWithoutAssignment = SemanticModel.AnalyzeDataFlow(nextStatementAfterAssignment, scopedInvocation);
                }
                catch (Exception e)
                {
                    return true;    //If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable 
                }

                if (flowAnalysisWithoutAssignment == null || !flowAnalysisWithoutAssignment.Succeeded)
                    return true;
                else if (countOfReachableCandidatesAfter > 0 &&
                         flowAnalysisWithoutAssignment.WrittenInside.Any(var => var.Name == VariableName))
                {
                    return false;
                }

                DataFlowAnalysis flowAnalysisWithAssignment = null;

                try
                {
                    flowAnalysisWithAssignment = SemanticModel.AnalyzeDataFlow(scopedAssignment, scopedInvocation);
                }
                catch (Exception e)
                {
                    return true;
                }

                if (flowAnalysisWithAssignment == null || !flowAnalysisWithAssignment.Succeeded)
                    return true;
                else if (flowAnalysisWithAssignment.AlwaysAssigned.All(var => var.Name != VariableName))
                    return false;

                return true;
            }

            private (bool IsSuccess, bool IsReacheable, StatementSyntax ScopedAssignment, StatementSyntax ScopedInvocation) 
                AnalyzeControlFlowBetweenAssignmentAndInvocation(StatementSyntax invocationStatement, StatementSyntax assignmentStatement)
            {
                ControlFlowAnalysis controlFlow = null;

                try
                {
                    controlFlow = SemanticModel.AnalyzeControlFlow(assignmentStatement);

                    if (controlFlow?.Succeeded == true && !controlFlow.EndPointIsReachable)
                        return (true, false, null, null);
                }
                catch (Exception e)
                {
                    //If there was some kind of error during analysis we should assume the worst case - that assignment is reacheable
                    // So do nothing
                }

                if (assignmentStatement.Parent.Equals(invocationStatement.Parent))
                {
                    return (true, true, assignmentStatement, invocationStatement);
                }

                var (commonAncestor, scopedAssignment, scopedInvocation) =
                    RoslynSyntaxUtils.LowestCommonAncestorSyntaxStatement(assignmentStatement, invocationStatement);

                if (commonAncestor != null && scopedAssignment != null && scopedInvocation != null)
                    return (true, true, scopedAssignment, scopedInvocation);

                return (false, false, null, null);
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

				public Stack<(StatementSyntax PotentialAssignment, TypeSyntax AssignedType)> Candidates { get; }
				
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
					Candidates = new Stack<(StatementSyntax, TypeSyntax)>(capacity: 2);
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
					   !(declarator.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation))
					{
						if (!IsCancelationRequested)
							base.VisitVariableDeclarator(declarator);

						return;
					}

					Candidates.Push((declarator.GetStatementNode(), objectCreation.Type));

					if (!IsCancelationRequested)
						base.VisitVariableDeclarator(declarator);
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
					{
						return;
					}

					if (!(curExpression is ObjectCreationExpressionSyntax objectCreation))
					{
						IsValid = false;
						return;
					}

					Candidates.Push((candidateAssignment.GetStatementNode(), objectCreation.Type));

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

                    try
                    {
                        ControlFlowAnalysis controlFlow = resolver.SemanticModel.AnalyzeControlFlow(invocation.GetStatementNode());

                        if (controlFlow?.Succeeded == true && !controlFlow.EndPointIsReachable)
                            return true;
                    }
                    catch (Exception e)
                    {
                        return false;
                    }

					return false;
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
			}
		}	
	}
}

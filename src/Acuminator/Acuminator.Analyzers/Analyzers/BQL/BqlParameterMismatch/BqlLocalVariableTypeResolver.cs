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
		protected class BqlLocalVariableTypeResolver : BqlInvocationDataFlowAnalyserBase
		{
			private readonly ResolveVarTypeMethodBodyWalker methodBodyWalker;

			public BqlLocalVariableTypeResolver(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext, IdentifierNameSyntax identifierNode) :
											base(syntaxContext, pxContext, identifierNode)
			{
				methodBodyWalker = new ResolveVarTypeMethodBodyWalker(this);
			}

			public ITypeSymbol ResolveVariableType()
			{
				if (CancellationToken.IsCancellationRequested)
					return null;

				MethodDeclarationSyntax methodDeclaration = Invocation.GetDeclaringMethodNode();

				if (methodDeclaration == null || !SemanticModel.IsLocalVariable(methodDeclaration, VariableName))
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

            private List<TypeSyntax> AnalyzeCandidates()
            {
                List<TypeSyntax> checkedCandidates = new List<TypeSyntax>(methodBodyWalker.Candidates.Count);

                while (methodBodyWalker.Candidates.Count > 0)
                {
                    var (potentialAssignmentStatement, assignedType) = methodBodyWalker.Candidates.Pop();

                    if (CanReachInvocation(potentialAssignmentStatement, checkedCandidates.Count))
                        checkedCandidates.Add(assignedType);
                }

                return checkedCandidates;
            }

            


            //*****************************************************************************************************************************************************************************
            //*****************************************************************************************************************************************************************************
            //*****************************************************************************************************************************************************************************
            private class ResolveVarTypeMethodBodyWalker : CSharpSyntaxWalker
			{
				private readonly BqlLocalVariableTypeResolver resolver;

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

					if (candidateAssignment == null || IsCancelationRequested || !(curExpression is ObjectCreationExpressionSyntax objectCreation))
						return;

					Candidates.Push((candidateAssignment.GetStatementNode(), objectCreation.Type));
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

                public override void VisitGotoStatement(GotoStatementSyntax node)
                {
                    if (node.IsKind(SyntaxKind.GotoStatement))   //The analysis is not valid if there are goto statements in method. This is rarely a case in C#
                    {
                        IsValid = false;
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

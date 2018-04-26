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

				MethodDeclarationSyntax methodDeclaration = GetDeclaringMethodNode(Invocation);

				if (methodDeclaration == null)
					return null;

				if (!IsLocalVariable(methodDeclaration))
					return null;

				methodBodyWalker.Visit(methodDeclaration);
			}

			private static MethodDeclarationSyntax GetDeclaringMethodNode(SyntaxNode node)
			{
				var current = node;

				while (current != null && !(current is MethodDeclarationSyntax))
				{
					current = current.Parent;
				}

				return current as MethodDeclarationSyntax;
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

				private bool isInVariableInvocation;

				private bool shouldStop;
				private bool isValid = true;

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
					if (resolver.CancellationToken.IsCancellationRequested)
					{
						IsValid = false;
					}

					if (shouldStop)
						return;

					base.Visit(node);
				}

				public override void VisitVariableDeclarator(VariableDeclaratorSyntax declarator)
				{
					if (resolver.CancellationToken.IsCancellationRequested || declarator.Identifier.ValueText != resolver.VariableName ||
					   !(declarator.Initializer?.Value is ObjectCreationExpressionSyntax objectCreation) || !IsReacheable(declarator))
					{
						if (!resolver.CancellationToken.IsCancellationRequested)
							base.VisitVariableDeclarator(declarator);

						return;
					}

					Candidates.Add((declarator, objectCreation.Type));

					if (!resolver.CancellationToken.IsCancellationRequested)
						base.VisitVariableDeclarator(declarator);
				}

				public override void VisitInvocationExpression(InvocationExpressionSyntax invocation)
				{
					if (invocation.IsEquivalentTo(resolver.Invocation))
					{
						shouldStop = true;
						return;
					}

					
						AnalyzeInvocationOnVariable(invocation);

						if (!IsValid)
							return;
					

					var arguments = invocation.ArgumentList.Arguments;

					if (arguments.Count == 0 || resolver.CancellationToken.IsCancellationRequested)
					{
						if (!resolver.CancellationToken.IsCancellationRequested)
							base.VisitInvocationExpression(invocation);

						return;
					}

					if (arguments.Any(arg => arg.NameColon?.Name?.Identifier.ValueText == resolver.VariableName))
					{
						IsValid = false;
						return;
					}


					if (!resolver.CancellationToken.IsCancellationRequested)
						base.VisitInvocationExpression(invocation);
				}			

				private void AnalyzeInvocationOnVariable(InvocationExpressionSyntax invocation)
				{
					//switch (invocation.Expression)
					//{
					//	case MemberAccessExpressionSyntax memberAccess when memberAccess.OperatorToken.Kind() == SyntaxKind.DotToken:
					//		if (!(memberAccess.Expression is IdentifierNameSyntax identifier) || identifier.Identifier.ValueText != resolver.VariableName)



					//		break;
					//	case MemberBindingExpressionSyntax memberBinding when memberBinding.OperatorToken.Kind() == SyntaxKind.DotToken:
					//		memberBinding.

					//		break;
					//	default:
					//		return;
					//}






					try
					{
						isInVariableInvocation = true;


					}
					finally
					{
						isInVariableInvocation = false;
					}
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private bool IsReacheable(SyntaxNode assignmentNode)
				{
					ControlFlowAnalysis flowAnalysis = resolver.SemanticModel.AnalyzeControlFlow(assignmentNode, resolver.Invocation);
					return flowAnalysis?.Succeeded == true && flowAnalysis.EndPointIsReachable;
				}
			}
		}	
	}
}

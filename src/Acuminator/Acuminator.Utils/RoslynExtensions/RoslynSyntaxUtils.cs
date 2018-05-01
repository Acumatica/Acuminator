using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Data;

namespace Acuminator.Utilities
{
	public static class RoslynSyntaxUtils
	{
		public static ExpressionSyntax GetAccessNodeFromInvocationNode(this InvocationExpressionSyntax invocationNode)
		{
			if (invocationNode == null)
				return null;

			if (invocationNode.Expression is MemberAccessExpressionSyntax memberAccessNode &&
				memberAccessNode.OperatorToken.Kind() == SyntaxKind.DotToken)
			{
				return memberAccessNode.Expression;
			}
			else if (invocationNode.Expression is MemberBindingExpressionSyntax memberBindingNode &&
					 memberBindingNode.OperatorToken.Kind() == SyntaxKind.DotToken &&
					 invocationNode.Parent is ConditionalAccessExpressionSyntax conditionalAccessNode)
			{
				return conditionalAccessNode.Expression;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool DoesArgumentsContainIdentifier(this InvocationExpressionSyntax invocation, string identifier)
		{
			invocation.ThrowOnNull(nameof(invocation));

			if (identifier.IsNullOrWhiteSpace())
				return false;

			var arguments = invocation.ArgumentList.Arguments;

			if (arguments.Count == 0)
				return false;

			return arguments.Select(arg => arg.Expression)
							.OfType<IdentifierNameSyntax>()
							.Any(arg => arg.Identifier.ValueText == identifier);
		}

		public static IEnumerable<ArgumentSyntax> GetArgumentsContainingIdentifier(this InvocationExpressionSyntax invocation, string identifier)
		{
			invocation.ThrowOnNull(nameof(invocation));

			if (identifier.IsNullOrWhiteSpace())
				yield break;

			var arguments = invocation.ArgumentList.Arguments;

			if (arguments.Count == 0)
				yield break;

			foreach (ArgumentSyntax argument in arguments)
			{
				if (argument.Expression is IdentifierNameSyntax identifierNameSyntax && identifierNameSyntax.Identifier.ValueText == identifier)
				{
					yield return argument;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static StatementSyntax GetStatementNode(this SyntaxNode node)
		{
			if (node == null)
				return null;

			SyntaxNode current = node;

			while (current != null && !(current is StatementSyntax))
			{
				current = current.Parent;
			}

			return current as StatementSyntax;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MethodDeclarationSyntax GetDeclaringMethodNode(this SyntaxNode node)
		{
			var current = node;

			while (current != null && !(current is MethodDeclarationSyntax))
			{
				current = current.Parent;
			}

			return current as MethodDeclarationSyntax;
		}

		public static StatementSyntax GetNextStatement(this StatementSyntax statement)
		{
			if (statement == null)
				return null;

			using (var enumerator = statement.Parent.ChildNodes().OfType<StatementSyntax>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					StatementSyntax curStatement = enumerator.Current;

					if (curStatement.Equals(statement))
					{
						if (enumerator.MoveNext())
						{
							return enumerator.Current;
						}
						else
						{
							switch (curStatement.Parent.Parent.Kind())
							{
								case SyntaxKind.MethodDeclaration:
								case SyntaxKind.OperatorDeclaration:
								case SyntaxKind.ConversionOperatorDeclaration:
								case SyntaxKind.ConstructorDeclaration:
								case SyntaxKind.DestructorDeclaration:
								case SyntaxKind.PropertyDeclaration:
								case SyntaxKind.EventDeclaration:
								case SyntaxKind.IndexerDeclaration:
								case SyntaxKind.GetAccessorDeclaration:
								case SyntaxKind.SetAccessorDeclaration:
								case SyntaxKind.AddAccessorDeclaration:
								case SyntaxKind.RemoveAccessorDeclaration:
								case SyntaxKind.UnknownAccessorDeclaration:
									return null;
								default:
									var parentStatement = curStatement.Parent.GetStatementNode();
									return parentStatement?.GetNextStatement();
							}
						}
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Get a parent with a specified type <typeparamref name="TParent"/>
		/// </summary>
		/// <typeparam name="TParent">Parent node type.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TParent Parent<TParent>(this SyntaxNode node)
		where TParent : SyntaxNode
		{
			if (node == null)
				return null;

			SyntaxNode curNode = node.Parent;

			while (curNode != null)
			{
				if (curNode is TParent parent)
					return parent;

				curNode = curNode.Parent;
			}

			return null;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the root.
		/// </summary>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth(this SyntaxNode node)
		{
			node.ThrowOnNull(nameof(node));

			int depth = 0;
			SyntaxNode curNode = node.Parent;

			while (curNode != null)
			{
				depth++;
				curNode = curNode.Parent;
			}

			return depth;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the first ancestor of type <typeparamref name="TRoot"/>. 
		/// If node doesn't have the ancestor of this type then depth from the root is returned.
		/// </summary>
		/// <typeparam name="TRoot">Type of the root.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth<TRoot>(this SyntaxNode node)
		where TRoot : SyntaxNode
		{
			node.ThrowOnNull(nameof(node));

			if (node is TRoot)
				return 0;

			int depth = 0;
			SyntaxNode curNode = node.Parent;

			while (curNode != null && !(curNode is TRoot))
			{
				depth++;
				curNode = curNode.Parent;
			}

			return depth;
		}

		/// <summary>
		/// Get depths of the given syntax node measuring from the first ancestor of type <typeparamref name="TRoot"/> and taking into account only nodes
		/// of type <typeparamref name="TNode"/>. If node doesn't have the ancestor of this type then the depth (taking into account only nodes of type
		/// <typeparamref name="TNode"/>) from the root is returned.
		/// </summary>
		/// <typeparam name="TRoot">Type of the root.</typeparam>
		/// <typeparam name="TNode">Type of the node.</typeparam>
		/// <param name="node">The node to act on.</param>
		/// <returns/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Depth<TRoot, TNode>(this TNode node)
		where TRoot : SyntaxNode
		where TNode : SyntaxNode
		{
			node.ThrowOnNull(nameof(node));

			if (node is TRoot)
				return 0;

			int depth = 0;
			TNode curNode = node.Parent<TNode>();

			while (curNode != null && !(curNode is TRoot))
			{
				depth++;
				curNode = curNode.Parent<TNode>();
			}

			return depth;
		}

		public static SyntaxNode LowestCommonAncestor(SyntaxNode nodeX, SyntaxNode nodeY)
		{
			nodeX.ThrowOnNull(nameof(nodeX));
			nodeY.ThrowOnNull(nameof(nodeY));

			int depthX = nodeX.Depth();            //Depth is average O(log n) operation, worst case is O(n) but it isn't the case for the syntax tree which is wide but not very deep
			int depthY = nodeY.Depth();

			SyntaxNode currentX = nodeX;
			SyntaxNode currentY = nodeY;
			
			while (depthX != depthY)				//First get nodes on the equal levels of depth
			{
				if (depthX > depthY)
				{
					currentX = currentX.Parent;
					depthX--;
				}
				else
				{
					currentY = currentY.Parent;
					depthY--;
				}
			}

			while (currentX != currentY)          //Then move up the branches until nodes coincide
			{
				currentX = currentX.Parent;
				currentY = currentY.Parent;
			}

			return currentX;
		}

		public static (StatementSyntax Ancestor, StatementSyntax ScopedX, StatementSyntax ScopedY) LowestCommonAncestorSyntaxStatement(StatementSyntax x, StatementSyntax y)
		{
			x.ThrowOnNull(nameof(x));
			y.ThrowOnNull(nameof(y));

			//Depth is average O(log n) operation, worst case is O(n) but it isn't the case for the syntax tree which is wide but not very deep.
			//For statements we could consider depth constrained by MethodDeclarationSyntax 
			int depthX = x.Depth<MethodDeclarationSyntax, StatementSyntax>();           
			int depthY = y.Depth<MethodDeclarationSyntax, StatementSyntax>();

			StatementSyntax currentX = x, prevX = null;
			StatementSyntax currentY = y, prevY = null;

			while (depthX != depthY)                //First get nodes on the equal levels of depth
			{
				if (depthX > depthY)
				{
					prevX = currentX;
					currentX = currentX.Parent<StatementSyntax>();
					depthX--;
				}
				else
				{
					prevY = currentY;
					currentY = currentY.Parent<StatementSyntax>();
					depthY--;
				}
			}

			while (currentX != currentY)          //Then move up the branches until nodes coincide
			{
				prevX = currentX;
				prevY = currentY;
				currentX = currentX.Parent<StatementSyntax>();
				currentY = currentY.Parent<StatementSyntax>();
			}

			return (currentX, prevX, prevY);
		}

		public static bool IsLocalVariable(this SemanticModel semanticModel, MethodDeclarationSyntax containingMethod, string variableName)
		{
			semanticModel.ThrowOnNull(nameof(semanticModel));
			containingMethod.ThrowOnNull(nameof(containingMethod));

			if (variableName.IsNullOrWhiteSpace())
				return false;

			DataFlowAnalysis dataFlowAnalysis = semanticModel.AnalyzeDataFlow(containingMethod.Body);

			if (dataFlowAnalysis == null || !dataFlowAnalysis.Succeeded)
				return false;

			return dataFlowAnalysis.VariablesDeclared.Any(var => var.Name == variableName);
		}
	}
}

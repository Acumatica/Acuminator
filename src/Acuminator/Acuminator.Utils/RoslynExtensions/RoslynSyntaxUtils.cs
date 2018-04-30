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
		public static bool ArgumentsContainIdentifier(this InvocationExpressionSyntax invocation, string identifier)
		{
			var arguments = invocation.ArgumentList.Arguments;

			if (arguments.Count == 0)
				return false;

			return arguments.Select(arg => arg.Expression)
							.OfType<IdentifierNameSyntax>()
							.Any(arg => arg.Identifier.ValueText == identifier);
		}

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

		public static int Depth(this SyntaxNode node)
		{
			node.ThrowOnNull(nameof(node));
			return node.Ancestors().Count();
		}

		public static SyntaxNode LowestCommonAncestor(SyntaxNode nodeX, SyntaxNode nodeY)
		{
			int depthX = nodeX.Depth();            //Depth is average O(log n) operation, worst case is O(n) but it isn't the case for the syntax tree which is wide but not very deep
			int depthY = nodeY.Depth();

			SyntaxNode curentX = nodeX;
			SyntaxNode currentY = nodeY;

			while (depthX != depthY)				//First get nodes on the equal levels of depth
			{
				if (depthX > depthY)
				{
					curentX = curentX.Parent;
					depthX--;
				}
				else
				{
					currentY = currentY.Parent;
					depthY--;
				}
			}

			while (curentX != currentY)          //Then move up the branches until nodes coincide
			{
				curentX = curentX.Parent;
				currentY = currentY.Parent;
			}

			return curentX;
		}
	}
}

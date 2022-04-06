#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public static class MethodDeclarationUtils
	{
		public static ExpressionSyntax? GetAccessNodeFromInvocationNode(this InvocationExpressionSyntax? invocationNode)
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

		/// <summary>
		/// Get method name location from invocation node.
		/// </summary>
		/// <param name="invocation">The invocation node to act on.</param>
		/// <returns>
		/// The method name's location.
		/// </returns>
		public static Location GetMethodNameLocation(this InvocationExpressionSyntax invocation)
		{
			invocation.ThrowOnNull(nameof(invocation));

			if (invocation.Expression is MemberAccessExpressionSyntax memberAccessNode)
			{
				return memberAccessNode.Name?.GetLocation() ?? invocation.GetLocation();
			}
			else if (invocation.Expression is MemberBindingExpressionSyntax memberBindingNode)
			{
				return memberBindingNode.Name?.GetLocation() ?? invocation.GetLocation();
			}

			return invocation.GetLocation();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool DoesArgumentsContainIdentifier(this InvocationExpressionSyntax invocation, string? identifier)
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

		public static IEnumerable<ArgumentSyntax> GetArgumentsContainingIdentifier(this InvocationExpressionSyntax invocation, string? identifier)
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
		public static StatementSyntax? GetStatementNode(this SyntaxNode? node)
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

		/// <summary>
		/// A <see cref="SyntaxNode"/> extension method that gets declaring method node for syntax nodes inside the method. For nodes outside the method returns <c>null</c>.
		/// </summary>
		/// <param name="node">The node to act on.</param>
		/// <returns>
		/// The declaring method node.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MethodDeclarationSyntax? GetDeclaringMethodNode(this SyntaxNode? node)
		{
			var current = node;

			while (current != null && !(current is MethodDeclarationSyntax))
			{
				current = current.Parent;
			}

			return current as MethodDeclarationSyntax;
		}

		public static StatementSyntax? GetNextStatement(this StatementSyntax? statement)
		{
			if (statement == null)
				return null;

			using var enumerator = statement.Parent.ChildNodes()
												   .OfType<StatementSyntax>()
												   .GetEnumerator();
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

			return null;
		}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsStatic(this BaseMethodDeclarationSyntax node) =>
			node.CheckIfNull(nameof(node))
				.Modifiers
				.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
	}
}

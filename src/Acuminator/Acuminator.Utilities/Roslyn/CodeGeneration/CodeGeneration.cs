using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for code generation.
	/// </summary>
	public static class CodeGeneration
	{
		/// <summary>
		/// Adds a missing using directive for namespace with name <paramref name="namespaceName"/> at the end of the using directives list.
		/// If the namespace is present does not return anything.
		/// </summary>
		/// <param name="root">The root node.</param>
		/// <param name="namespaceName">Name of the namespace.</param>
		/// <returns>
		/// The root node with added using directive.
		/// </returns>
		public static CompilationUnitSyntax AddMissingUsingDirectiveForNamespace(this CompilationUnitSyntax root, string namespaceName)
		{
			root.ThrowOnNull();
			namespaceName.ThrowOnNullOrWhiteSpace();

			bool alreadyHasUsing = root.Usings.Any(usingDirective => namespaceName == usingDirective.Name?.ToString());

			if (alreadyHasUsing)
				return root;

			return root.AddUsings(
							UsingDirective(
								ParseName(namespaceName)));
		}

		/// <summary>
		/// Create attribute list of the supplied type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static AttributeListSyntax GetAttributeList(this INamedTypeSymbol type, AttributeArgumentListSyntax? argumentList = null)
		{
			type.ThrowOnNull();

			var node = Attribute(
						IdentifierName(
							type.Name))
						.WithAdditionalAnnotations(Simplifier.Annotation);

			if (argumentList != null)
			{
				node = node.WithArgumentList(argumentList);
			}

			var list = AttributeList(
						SingletonSeparatedList(
							node));

			return list;
		}

		/// <summary>
		/// Removes the regions from the type member node leading trivia.
		/// </summary>
		/// <param name="member">The type member node.</param>
		/// <returns>
		/// Type member node with removed regions from leading trivia.
		/// </returns>
		[return: NotNullIfNotNull(nameof(member))]
		public static MemberDeclarationSyntax? RemoveRegionsFromLeadingTrivia(MemberDeclarationSyntax? member)
		{
			if (member == null)
				return member;

			var leadingTrivia = member.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return member;

			var regionTrivias = leadingTrivia.GetRegionDirectiveLinesFromTrivia();

			if (regionTrivias.Count == 0)
				return member;

			var newLeadingTrivia = leadingTrivia.Except(regionTrivias);
			return member.WithLeadingTrivia(newLeadingTrivia);
		}
	}
}

using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public static class SyntaxNodeGenerationUtils
	{
		/// <summary>
		/// Create attribute list of the supplied type
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static AttributeListSyntax GetAttributeList(this INamedTypeSymbol type, AttributeArgumentListSyntax argumentList = null)
		{
			type.ThrowOnNull(nameof(type));

			var node = SyntaxFactory.Attribute(
				SyntaxFactory.IdentifierName(
					type.Name))
				.WithAdditionalAnnotations(Simplifier.Annotation);

			if (argumentList != null)
			{
				node = node.WithArgumentList(argumentList);
			}

			var list = SyntaxFactory.AttributeList(
				SyntaxFactory.SingletonSeparatedList(
					node));

			return list;
		}
	}
}

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.Helpers
{
	internal static class AnalyzerHelper
	{
		internal static SyntaxTokenList CreateTokenListWithAccessibilityModifier(
			SyntaxKind accessibilityModifier,
			SyntaxTokenList existingModifiers,
			Func<SyntaxKind, bool> filter,
			bool addVirtual)
		{
			var index = -1;
			for (var i = 0; i < existingModifiers.Count; i++)
			{
				if (filter(existingModifiers[i].Kind()))
				{
					index = i;
					break;
				}
			}

			var newToken = SyntaxFactory.Token(accessibilityModifier);
			if (index > -1)
			{
				newToken = newToken.WithTriviaFrom(existingModifiers[index]);
			}

			var newModifiers = existingModifiers.Where(m => !filter(m.Kind()));

			var syntaxModifiers = SyntaxFactory.TokenList(newToken);

			if (addVirtual)
			{
				syntaxModifiers = syntaxModifiers.Add(SyntaxFactory.Token(SyntaxKind.VirtualKeyword));
			}

			syntaxModifiers = syntaxModifiers.AddRange(newModifiers);

			return syntaxModifiers;
		}
	}
}

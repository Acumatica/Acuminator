using System;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Acuminator.Analyzers.StaticAnalysis.EventHandlerModifier.Helpers
{
	internal static class AnalyzerHelper
	{
		internal static bool ImplementsInterface(IMethodSymbol? methodSymbol)
		{
			return methodSymbol?.ContainingType.AllInterfaces.SelectMany(i => i.GetMethods(methodSymbol.Name)).Any(m => SignaturesMatch(m, methodSymbol)) ?? false;
		}

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

		private static bool SignaturesMatch(IMethodSymbol method, IMethodSymbol other)
		{
			if (method.Name != other.Name)
			{
				return false;
			}

			if (method.Parameters.Length != other.Parameters.Length)
			{
				return false;
			}

			if (!method.ReturnType.Equals(other.ReturnType, SymbolEqualityComparer.Default))
			{
				return false;
			}

			if (method.Arity != other.Arity)
			{
				return false;
			}

			if (method.IsGenericMethod != other.IsGenericMethod)
			{
				return false;
			}

			if (method.IsGenericMethod)
			{
				if (method.TypeArguments.Length != other.TypeArguments.Length)
				{
					return false;
				}

				for (int i = 0; i < method.TypeArguments.Length; i++)
				{
					if (!method.TypeArguments[i].Equals(other.TypeArguments[i], SymbolEqualityComparer.Default))
					{
						return false;
					}
				}
			}

			for (int i = 0; i < method.Parameters.Length; i++)
			{
				if (!method.Parameters[i].Type.Equals(other.Parameters[i].Type, SymbolEqualityComparer.Default))
				{
					return false;
				}
			}

			return true;
		}
	}
}

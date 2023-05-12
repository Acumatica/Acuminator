#nullable enable

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	public static class PXOverrideMethodSymbolComparer
	{
		/// <summary>
		/// Special signature check between a derived method with the PXOverride attribute and the base method.
		/// There are two cases:
		/// 1. Simple case: Method signatures are identical.
		/// 2. Complex case: Method signatures are identical, but there is an additional method parameter that is a delegate type that points to the base implementation.
		/// 
		/// In both cases, PXOverride dictates that the base method must be
		///     1) virtual (having either a virtual or an override signature),
		///     2) public, protected or protected internal.
		/// </summary>
		/// <param name="sourceMethod">The method from the base</param>
		/// <param name="extensionMethod">The method from the derived class, with the PXOverride attribute</param>
		/// <returns></returns>
		public static bool Equals(IMethodSymbol sourceMethod, IMethodSymbol extensionMethod)
		{
			var isSimple = sourceMethod.Parameters.Length == extensionMethod.Parameters.Length;
			var isComplex = sourceMethod.Parameters.Length + 1 == extensionMethod.Parameters.Length;

			if (!isSimple && !isComplex)
			{
				return false;
			}

			if (!HasCorrectOverrideSignature(sourceMethod) || !HasCorrectAccessibility(sourceMethod))
			{
				return false;
			}

			if (!sourceMethod.ReturnType.Equals(extensionMethod.ReturnType))
			{
				return false;
			}

			var sourceMethodParameterTypes = sourceMethod.Parameters.Select(p => p.Type);
			var extensionMethodParameterTypes = extensionMethod.Parameters.Select(p => p.Type);

			if (isSimple)
			{
				return sourceMethodParameterTypes.SequenceEqual(extensionMethodParameterTypes);
			}

			if (extensionMethodParameterTypes.Last() is not INamedTypeSymbol @delegate)
			{
				return false;
			}

			if (!sourceMethodParameterTypes.SequenceEqual(extensionMethodParameterTypes.Take(extensionMethod.Parameters.Length - 1)))
			{
				return false;
			}

			return DelegateSignatureMatches(sourceMethod.ReturnType, sourceMethodParameterTypes, @delegate);
		}

		private static bool HasCorrectOverrideSignature(IMethodSymbol methodSymbol) => methodSymbol.IsVirtual || methodSymbol.IsOverride;

		private static bool HasCorrectAccessibility(IMethodSymbol methodSymbol) =>
			methodSymbol.DeclaredAccessibility == Accessibility.Public ||
			methodSymbol.DeclaredAccessibility == Accessibility.Protected ||
			methodSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

		private static bool DelegateSignatureMatches(ITypeSymbol sourceMethodReturnType, IEnumerable<ITypeSymbol> sourceMethodParameterTypes, INamedTypeSymbol targetDelegate)
		{
			if (targetDelegate.TypeKind != TypeKind.Delegate)
			{
				return false;
			}

			if (!sourceMethodReturnType.Equals(targetDelegate.DelegateInvokeMethod.ReturnType))
			{
				return false;
			}

			if (!sourceMethodParameterTypes.SequenceEqual(targetDelegate.DelegateInvokeMethod.Parameters.Select(p => p.Type)))
			{
				return false;
			}

			return true;
		}
	}
}

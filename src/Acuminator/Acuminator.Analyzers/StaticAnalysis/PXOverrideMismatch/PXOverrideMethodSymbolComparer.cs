#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	public class PXOverrideMethodSymbolComparer : IEqualityComparer<IMethodSymbol>
	{
		public static readonly PXOverrideMethodSymbolComparer Instance = new();

		public bool Equals(IMethodSymbol sourceMethod, IMethodSymbol extensionMethod)
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

			if (!sourceMethodParameterTypes.SequenceEqual(isSimple ? extensionMethodParameterTypes : extensionMethodParameterTypes.Take(extensionMethod.Parameters.Length - 1)))
			{
				return false;
			}

			if (!isComplex)
			{
				return true;
			}

			if (extensionMethodParameterTypes.Last() is not INamedTypeSymbol @delegate)
			{
				return false;
			}

			return IsDelegateCompatible(sourceMethod.ReturnType, sourceMethodParameterTypes, @delegate);
		}

		public int GetHashCode(IMethodSymbol obj) => obj == null ? 0 : obj.GetHashCode();

		private static bool HasCorrectOverrideSignature(IMethodSymbol methodSymbol) => methodSymbol.IsVirtual || methodSymbol.IsOverride;

		private static bool HasCorrectAccessibility(IMethodSymbol methodSymbol) =>
			methodSymbol.DeclaredAccessibility == Accessibility.Public ||
			methodSymbol.DeclaredAccessibility == Accessibility.Protected ||
			methodSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

		private static bool IsDelegateCompatible(ITypeSymbol sourceMethodReturnType, IEnumerable<ITypeSymbol> sourceMethodParameterTypes, INamedTypeSymbol targetDelegate)
		{
			if (!targetDelegate.IsGenericType && targetDelegate.TypeKind != TypeKind.Delegate)
			{
				return false;
			}

			var returnType = targetDelegate.TypeKind == TypeKind.Delegate ? targetDelegate.DelegateInvokeMethod.ReturnType : targetDelegate.TypeArguments.Last();

			if (!returnType.Equals(sourceMethodReturnType))
			{
				return false;
			}

			var delegateArguments = targetDelegate.TypeKind == TypeKind.Delegate ?
				targetDelegate.DelegateInvokeMethod.Parameters.Select(p => p.Type).ToImmutableArray() :
				targetDelegate.TypeArguments.Take(targetDelegate.TypeArguments.Length - 1).ToImmutableArray();

			if (!sourceMethodParameterTypes.SequenceEqual(delegateArguments))
			{
				return false;
			}

			return true;
		}
	}
}

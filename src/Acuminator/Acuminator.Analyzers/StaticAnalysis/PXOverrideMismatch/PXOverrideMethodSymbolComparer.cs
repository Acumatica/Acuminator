using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using System.Linq;
using Acuminator.Utilities.Common;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverrideMismatch
{
	public class PXOverrideMethodSymbolComparer : IEqualityComparer<IMethodSymbol>
	{
		public static PXOverrideMethodSymbolComparer Instance = new PXOverrideMethodSymbolComparer();

		public bool Equals(IMethodSymbol src, IMethodSymbol ext)
		{
			var srcParams = src.Parameters.Select(p => p.Type).ToImmutableArray();
			var extParams = ext.Parameters.Select(p => p.Type).ToImmutableArray();

			var isSimple = srcParams.Length == extParams.Length;
			var isComplex = srcParams.Length + 1 == extParams.Length;

			if (!isSimple && !isComplex)
			{
				return false;
			}

			if (!HasCorrectOverrideSignature(src) || !HasCorrectAccessibility(src))
			{
				return false;
			}

			if (!src.ReturnType.Equals(ext.ReturnType))
			{
				return false;
			}

			if (!DoParametersMatch(srcParams, extParams))
			{
				return false;
			}

			if (!isComplex)
			{
				return true;
			}

			if (extParams.Last() is not INamedTypeSymbol del)
			{
				return false;
			}

			return IsDelegateCompatible(src.ReturnType, srcParams, del);
		}

		public int GetHashCode(IMethodSymbol obj) => obj == null ? 0 : obj.GetHashCode();

		private static bool HasCorrectOverrideSignature(IMethodSymbol methodSymbol) => methodSymbol.IsVirtual || methodSymbol.IsOverride;

		private static bool HasCorrectAccessibility(IMethodSymbol methodSymbol) =>
			methodSymbol.DeclaredAccessibility == Accessibility.Public ||
			methodSymbol.DeclaredAccessibility == Accessibility.Protected ||
			methodSymbol.DeclaredAccessibility == Accessibility.ProtectedOrInternal;

		private static bool IsDelegateCompatible(ITypeSymbol srcReturnType, ImmutableArray<ITypeSymbol> srcParameters, INamedTypeSymbol targetDelegate)
		{
			if (!targetDelegate.IsGenericType && targetDelegate.TypeKind != TypeKind.Delegate)
			{
				return false;
			}

			var returnType = targetDelegate.TypeKind == TypeKind.Delegate ? targetDelegate.DelegateInvokeMethod.ReturnType : targetDelegate.TypeArguments.Last();

			if (!returnType.Equals(srcReturnType))
			{
				return false;
			}

			var delegateArguments = targetDelegate.TypeKind == TypeKind.Delegate ?
				targetDelegate.DelegateInvokeMethod.Parameters.Select(p => p.Type).ToImmutableArray() :
				targetDelegate.TypeArguments.Take(targetDelegate.TypeArguments.Length - 1).ToImmutableArray();

			if (srcParameters.Length != delegateArguments.Length)
			{
				return false;
			}

			if (!DoParametersMatch(srcParameters, delegateArguments))
			{
				return false;
			}

			return true;
		}

		private static bool DoParametersMatch(ImmutableArray<ITypeSymbol> srcParameters, ImmutableArray<ITypeSymbol> targetParameters)
		{
			for (var i = 0; i < srcParameters.Length; i++)
			{
				if (!srcParameters[i].Equals(targetParameters[i]))
				{
					return false;
				}
			}

			return true;
		}
	}
}

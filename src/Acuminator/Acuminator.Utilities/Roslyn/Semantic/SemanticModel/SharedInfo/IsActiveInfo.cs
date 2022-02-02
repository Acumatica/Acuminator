#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.SharedInfo
{
	/// <summary>
	/// Information about the IsActive special method in graph and DAC extensions.
	/// </summary>
	public class IsActiveInfo : SymbolItem<IMethodSymbol>
	{
		public IsActiveInfo(IMethodSymbol isActiveMethod) :
					   base(isActiveMethod, declarationOrder: 0)
		{
		}

		/// <summary>
		/// Collects info about IsActive method from DAC or graph extension symbol and creates <see cref="IsActiveInfo"/> DTO.
		/// </summary>
		/// <param name="dacOrGraphExtension">The DAC or graph extension.</param>
		/// <returns>
		/// The <see cref="IsActiveInfo"/> DTO if extension contains IsActive method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveInfo? GetIsActiveInfo(INamedTypeSymbol dacOrGraphExtension)
		{
			ImmutableArray<ISymbol> isActiveCandidates = dacOrGraphExtension.GetMembers(DelegateNames.IsActive);

			if (isActiveCandidates.IsDefaultOrEmpty)
				return null;

			IMethodSymbol isActiveMethod =
				isActiveCandidates.OfType<IMethodSymbol>()
								  .FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
															method.Parameters.IsDefaultOrEmpty && !method.IsGenericMethod &&
															method.ReturnType.SpecialType == SpecialType.System_Boolean);
			return isActiveMethod != null
				? new IsActiveInfo(isActiveMethod)
				: null;
		}
	}
}
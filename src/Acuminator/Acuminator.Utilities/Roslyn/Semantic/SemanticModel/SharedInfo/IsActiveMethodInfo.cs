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
	public class IsActiveMethodInfo : SymbolItem<IMethodSymbol>
	{
		private const int IsActiveDeclarationOrderToPlaceItFirst= -1;

		public IsActiveMethodInfo(IMethodSymbol isActiveMethod, int? declarationOrder = null) :
							 base(isActiveMethod, declarationOrder: declarationOrder ?? IsActiveDeclarationOrderToPlaceItFirst)
		{
		}

		/// <summary>
		/// Collects info about IsActive method from DAC or graph extension symbol and creates <see cref="IsActiveMethodInfo"/> DTO.
		/// </summary>
		/// <param name="dacOrGraphExtension">The DAC or graph extension.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// The <see cref="IsActiveMethodInfo"/> DTO if extension contains IsActive method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveMethodInfo? GetIsActiveMethodInfo(INamedTypeSymbol dacOrGraphExtension, int? declarationOrder = null)
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
				? new IsActiveMethodInfo(isActiveMethod, declarationOrder)
				: null;
		}
	}
}
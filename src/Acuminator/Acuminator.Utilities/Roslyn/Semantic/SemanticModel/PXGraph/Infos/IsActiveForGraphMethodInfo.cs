#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the IsActiveForGraph&lt;TGraph&gt; special method in graph extensions.
	/// </summary>
	public class IsActiveForGraphMethodInfo : SymbolItem<IMethodSymbol>
	{
		/// <summary>
		/// (Immutable) The IsActiveForGraph&lt;TGraph&gt; declaration order to place it second after IsActive .
		/// </summary>
		private const int IsActiveForGraphDeclarationOrderToPlaceItSecond = -1;

		public IsActiveForGraphMethodInfo(IMethodSymbol isActiveForGraphMethod, int? declarationOrder = null) :
									 base(isActiveForGraphMethod, declarationOrder: declarationOrder ?? IsActiveForGraphDeclarationOrderToPlaceItSecond)
		{
		}

		/// <summary>
		/// Collects info about IsActiveForGraph&lt;TGraph&gt; method from a graph extension symbol and creates <see cref="IsActiveForGraphMethodInfo"/> DTO.
		/// </summary>
		/// <param name="graphExtension">The graph extension.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// The <see cref="IsActiveForGraphMethodInfo"/> DTO if the graph extension contains IsActiveForGraph&lt;TGraph&gt; method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo(INamedTypeSymbol graphExtension, int? declarationOrder = null)
		{
			ImmutableArray<ISymbol> isActiveForGraphCandidates = graphExtension.GetMembers(DelegateNames.IsActiveForGraph);

			if (isActiveForGraphCandidates.IsDefaultOrEmpty)
				return null;

			IMethodSymbol isActiveForGraphMethod =
				isActiveForGraphCandidates.OfType<IMethodSymbol>()
										  .FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
																	method.Parameters.IsDefaultOrEmpty && 
																	method.ReturnType.SpecialType == SpecialType.System_Boolean &&
																	method.TypeParameters.Length == 1);
			return isActiveForGraphMethod != null
				? new IsActiveForGraphMethodInfo(isActiveForGraphMethod, declarationOrder)
				: null;
		}
	}
}
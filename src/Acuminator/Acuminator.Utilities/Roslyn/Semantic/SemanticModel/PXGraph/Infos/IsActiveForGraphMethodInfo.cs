#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the IsActiveForGraph&lt;TGraph&gt; special method in graph extensions.
	/// </summary>
	public class IsActiveForGraphMethodInfo : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		/// <summary>
		/// (Immutable) The IsActiveForGraph&lt;TGraph&gt; declaration order to place it second after IsActive .
		/// </summary>
		private const int IsActiveForGraphDeclarationOrderToPlaceItSecond = -1;

		public IsActiveForGraphMethodInfo(MethodDeclarationSyntax node, IMethodSymbol isActiveForGraphMethod, int? declarationOrder = null) :
									 base(node, isActiveForGraphMethod, declarationOrder ?? IsActiveForGraphDeclarationOrderToPlaceItSecond)
		{
		}

		/// <summary>
		/// Collects info about IsActiveForGraph&lt;TGraph&gt; method from a graph extension symbol and creates <see cref="IsActiveForGraphMethodInfo"/> DTO.
		/// </summary>
		/// <param name="graphExtension">The graph extension.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// The <see cref="IsActiveForGraphMethodInfo"/> DTO if the graph extension contains IsActiveForGraph&lt;TGraph&gt; method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo(INamedTypeSymbol graphExtension, CancellationToken cancellationToken, 
																				  int? declarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<ISymbol> isActiveForGraphCandidates = graphExtension.GetMembers(DelegateNames.IsActiveForGraph);

			if (isActiveForGraphCandidates.IsDefaultOrEmpty)
				return null;

			IMethodSymbol? isActiveForGraphMethod =
				isActiveForGraphCandidates.OfType<IMethodSymbol>()
										  .FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
																	method.Parameters.IsDefaultOrEmpty && 
																	method.ReturnType.SpecialType == SpecialType.System_Boolean &&
																	method.TypeParameters.Length == 1);

			SyntaxReference? isActiveForGraphMethodReference = isActiveForGraphMethod?.DeclaringSyntaxReferences.FirstOrDefault();

			return isActiveForGraphMethodReference?.GetSyntax(cancellationToken) is MethodDeclarationSyntax isActiveForGraphMethodNode
				? new IsActiveForGraphMethodInfo(isActiveForGraphMethodNode, isActiveForGraphMethod!, declarationOrder)
				: null;
		}
	}
}
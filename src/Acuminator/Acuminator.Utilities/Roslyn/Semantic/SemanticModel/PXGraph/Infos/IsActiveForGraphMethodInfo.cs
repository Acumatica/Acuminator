#nullable enable

using System;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;

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
		/// The IsActiveForGraph&lt;TGraph&gt; declaration order to place it second after IsActive.
		/// </summary>
		internal const int IsActiveForGraphDeclarationOrderToPlaceItSecond = IsActiveMethodInfo.IsActiveDeclarationOrderToPlaceItFirst + 1;

		public IsActiveForGraphMethodInfo(MethodDeclarationSyntax? node, IMethodSymbol isActiveForGraphMethod, int declarationOrder) :
									 base(node, isActiveForGraphMethod, declarationOrder)
		{
		}

		/// <summary>
		/// Collects info about IsActiveForGraph&lt;TGraph&gt; method from a graph extension symbol and creates <see cref="IsActiveForGraphMethodInfo"/> DTO.
		/// </summary>
		/// <param name="graphExtension">The graph extension.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="customDeclarationOrder">(Optional) The declaration order. Default value is <see cref="IsActiveForGraphDeclarationOrderToPlaceItSecond"/>.</param>
		/// <returns>
		/// The <see cref="IsActiveForGraphMethodInfo"/> DTO if the graph extension contains IsActiveForGraph&lt;TGraph&gt; method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveForGraphMethodInfo? GetIsActiveForGraphMethodInfo(INamedTypeSymbol graphExtension, CancellationToken cancellationToken,
																				  int? customDeclarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();

			int declarationOrder				  = customDeclarationOrder ?? IsActiveForGraphDeclarationOrderToPlaceItSecond;
			var isActiveForGraphCandidates		  = graphExtension.GetMethods(DelegateNames.IsActiveForGraph);
			IMethodSymbol? isActiveForGraphMethod =
				isActiveForGraphCandidates.FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
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
#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.SharedInfo
{
	/// <summary>
	/// Information about the IsActive special method in graph and DAC extensions.
	/// </summary>
	public class IsActiveMethodInfo : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		private const int IsActiveDeclarationOrderToPlaceItFirst= -1;

		public IsActiveMethodInfo(MethodDeclarationSyntax node, IMethodSymbol isActiveMethod, int? declarationOrder = null) :
							 base(node, isActiveMethod, declarationOrder ?? IsActiveDeclarationOrderToPlaceItFirst)
		{
		}

		/// <summary>
		/// Collects info about IsActive method from DAC or graph extension symbol and creates <see cref="IsActiveMethodInfo"/> DTO.
		/// </summary>
		/// <param name="dacOrGraphExtension">The DAC or graph extension.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// The <see cref="IsActiveMethodInfo"/> DTO if extension contains IsActive method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveMethodInfo? GetIsActiveMethodInfo(INamedTypeSymbol dacOrGraphExtension, CancellationToken cancellationToken,
																  int? declarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ImmutableArray<ISymbol> isActiveCandidates = dacOrGraphExtension.GetMembers(DelegateNames.IsActive);

			if (isActiveCandidates.IsDefaultOrEmpty)
				return null;
		
			IMethodSymbol? isActiveMethod =
				isActiveCandidates.OfType<IMethodSymbol>()
								  .FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
															method.Parameters.IsDefaultOrEmpty && !method.IsGenericMethod &&
															method.ReturnType.SpecialType == SpecialType.System_Boolean);

			SyntaxReference? isActiveMethodReference = isActiveMethod?.DeclaringSyntaxReferences.FirstOrDefault();

			return isActiveMethodReference?.GetSyntax(cancellationToken) is MethodDeclarationSyntax isActiveMethodNode
				? new IsActiveMethodInfo(isActiveMethodNode, isActiveMethod!, declarationOrder)
				: null;
		}
	}
}
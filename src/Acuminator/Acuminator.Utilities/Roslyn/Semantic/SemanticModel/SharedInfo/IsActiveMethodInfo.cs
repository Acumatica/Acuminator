#nullable enable

using System;
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
		internal const int IsActiveDeclarationOrderToPlaceItFirst = int.MinValue;

		public IsActiveMethodInfo(MethodDeclarationSyntax node, IMethodSymbol isActiveMethod, int declarationOrder) :
							 base(node, isActiveMethod, declarationOrder)
		{
		}

		/// <summary>
		/// Collects info about IsActive method from DAC or graph extension symbol and creates <see cref="IsActiveMethodInfo"/> DTO.
		/// </summary>
		/// <param name="dacOrGraphExtension">The DAC or graph extension.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="customDeclarationOrder">(Optional) The declaration order. Default value is <see cref="IsActiveDeclarationOrderToPlaceItFirst"/>.</param>
		/// <returns>
		/// The <see cref="IsActiveMethodInfo"/> DTO if extension contains IsActive method, otherwise <see langword="null"/>.
		/// </returns>
		internal static IsActiveMethodInfo? GetIsActiveMethodInfo(INamedTypeSymbol dacOrGraphExtension, CancellationToken cancellationToken,
																  int? customDeclarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();

			int declarationOrder   = customDeclarationOrder ?? IsActiveDeclarationOrderToPlaceItFirst;
			var isActiveCandidates = dacOrGraphExtension.GetMethods(DelegateNames.IsActive);
			IMethodSymbol? isActiveMethod =
				isActiveCandidates.FirstOrDefault(method => method.IsStatic && method.DeclaredAccessibility == Accessibility.Public &&
															method.Parameters.IsDefaultOrEmpty && !method.IsGenericMethod &&
															method.ReturnType.SpecialType == SpecialType.System_Boolean);

			SyntaxReference? isActiveMethodReference = isActiveMethod?.DeclaringSyntaxReferences.FirstOrDefault();

			return isActiveMethodReference?.GetSyntax(cancellationToken) is MethodDeclarationSyntax isActiveMethodNode
				? new IsActiveMethodInfo(isActiveMethodNode, isActiveMethod!, declarationOrder)
				: null;
		}
	}
}
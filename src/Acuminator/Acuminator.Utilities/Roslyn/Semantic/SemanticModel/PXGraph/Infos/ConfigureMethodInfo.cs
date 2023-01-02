#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// Information about the Configure special method in graph extensions.
	/// </summary>
	/// <remarks>
	/// <see cref="ConfigureMethodInfo"/> is derived from <see cref="SymbolItem{T}"/> insted of <see cref="NodeSymbolItem{N, S}"/>.
	/// This way the class does not keep any information about syntax nodes.<br/>
	/// This is done intentionally to support a scenario when Configure method is overriden in a customization with no access to source code with base overrides.
	/// </remarks>
	public class ConfigureMethodInfo : SymbolItem<IMethodSymbol>
	{
		/// <summary>
		/// The Configure method declaration order to place it second after IsActiveForGraph&lt;TGraph&gt;.
		/// </summary>
		private const int ConfigureDeclarationOrderToPlaceItThird = -1;

		public ConfigureMethodInfo(IMethodSymbol configureMethod, int? declarationOrder = null) :
							  base(configureMethod, declarationOrder ?? ConfigureDeclarationOrderToPlaceItThird)
		{
		}

		/// <summary>
		/// Collects info about Configure method overrides from a graph extension symbol and creates <see cref="ConfigureMethodInfo"/> DTO.
		/// </summary>
		/// <param name="graphExtension">The graph extension.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// A collection of <see cref="ConfigureMethodInfo"/> DTOs if the graph extension contains one or several Configure method, otherwise an empty collection.
		/// </returns>
		internal static IReadOnlyCollection<ConfigureMethodInfo> GetConfigureMethodInfos(INamedTypeSymbol graphExtension, PXContext pxContext, 
																						 CancellationToken cancellationToken, int? declarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (pxContext?.PXGraphExtension.Configure == null)
				return Array.Empty<ConfigureMethodInfo>();

			ImmutableArray<ISymbol> configureTypeMembers = graphExtension.GetMembers(DelegateNames.Workflow.Configure);

			if (configureTypeMembers.IsDefaultOrEmpty)
				return Array.Empty<ConfigureMethodInfo>();

			var configureCandidates = from method in configureTypeMembers.OfType<IMethodSymbol>()
									  where !method.IsStatic && method.ReturnsVoid && method.IsOverride && 
											 method.DeclaredAccessibility == Accessibility.Public && method.Parameters.Length == 1
									  select method;

			foreach (IMethodSymbol configureMethodCandidate in configureCandidates)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var overridesChain = configureMethodCandidate.GetOverridesAndThis().ToList();
				var originalVirtualMethod = overridesChain[overridesChain.Count - 1];

				if (pxContext.PXGraphExtension.Configure.Equals(originalVirtualMethod))
				{
					// Do not include the original PXGraphExtension.Configure method into results
					return overridesChain.Take(overridesChain.Count - 1)
										 .Select(configureMethodOverride => new ConfigureMethodInfo(configureMethodOverride, declarationOrder))
										 .ToList(capacity: overridesChain.Count - 1);
				} 
			}

			return Array.Empty<ConfigureMethodInfo>();
		}
	}
}
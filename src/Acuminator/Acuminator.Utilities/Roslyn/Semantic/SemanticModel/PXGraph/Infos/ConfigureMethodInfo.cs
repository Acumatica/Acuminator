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
		/// Collects info about Configure method overrides from a graph or graph extension symbol and creates <see cref="ConfigureMethodInfo"/> DTO.
		/// </summary>
		/// <remarks>
		/// We collect only Configure method overrides from the class hierarchy because:<br/>
		/// - PXOverride mechanism is not supported by the workflow mechanism. Thus, it's no use to check chained extensions, <bt/>  
		/// they can't affect workflow configuration done by the base extension or graph
		/// - Graph and graph extension overrides of Configure method can be considered independently.<br/>  
		/// Therefore, for graph extension base graph's Configure method overrides are not included into results.
		/// </remarks>
		/// <param name="graphOrGraphExtension">The graph or graph extension.</param>
		/// <param name="graphType">Type of the <paramref name="graphOrGraphExtension"/> symbol.</param>
		/// <param name="pxContext">The Acumatica context.</param>
		/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
		/// <param name="declarationOrder">(Optional) The declaration order.</param>
		/// <returns>
		/// A collection of <see cref="ConfigureMethodInfo"/> DTOs if the graph / graph extension contains one or several Configure method, otherwise an empty collection.
		/// </returns>
		internal static IReadOnlyCollection<ConfigureMethodInfo> GetConfigureMethodInfos(INamedTypeSymbol graphOrGraphExtension, GraphType graphType, PXContext pxContext,
																						 CancellationToken cancellationToken, int? declarationOrder = null)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var originalConfigureMethod = GetOriginalConfigureMethod(graphType, pxContext);

			if (originalConfigureMethod == null)
				return Array.Empty<ConfigureMethodInfo>();

			var allConfigureTypeMethods = (from type in graphOrGraphExtension.GetBaseTypesAndThis()
										   select type.GetMembers(DelegateNames.Workflow.Configure)
												into configureTypeMembers
										   where !configureTypeMembers.IsDefaultOrEmpty
										   select configureTypeMembers.OfType<IMethodSymbol>())
										  .SelectMany(m => m);

			var configureCandidates = from method in allConfigureTypeMethods
									  where !method.IsStatic && method.ReturnsVoid && method.IsOverride && 
											 method.DeclaredAccessibility == Accessibility.Public && method.Parameters.Length == 1
									  select method;

			foreach (IMethodSymbol configureMethodCandidate in configureCandidates)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var overridesChain = configureMethodCandidate.GetOverridesAndThis().ToList();
				var originalVirtualMethod = overridesChain[overridesChain.Count - 1];

				if (originalConfigureMethod.Equals(originalVirtualMethod))
				{
					// Do not include the original PXGraphExtension.Configure method into results
					return overridesChain.Take(overridesChain.Count - 1)
										 .Select(configureMethodOverride => new ConfigureMethodInfo(configureMethodOverride, declarationOrder))
										 .ToList(capacity: overridesChain.Count - 1);
				} 
			}

			return Array.Empty<ConfigureMethodInfo>();
		}

		private static IMethodSymbol? GetOriginalConfigureMethod(GraphType graphType, PXContext pxContext) =>
			graphType switch
			{
				GraphType.PXGraph => pxContext?.PXGraph.Configure,
				GraphType.PXGraphExtension => pxContext?.PXGraphExtension.Configure,
				_ => null
			};
	}
}
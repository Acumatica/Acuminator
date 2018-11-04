using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to filter out views which are read only if graph has non read only views.
	/// </summary>
	public class NoReadOnlyViewGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public NoReadOnlyViewGraphRule(double? weight = null) : base(weight)
		{
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return Enumerable.Empty<ITypeSymbol>();

			List<INamedTypeSymbol> readOnlyViews = new List<INamedTypeSymbol>(capacity: 4);
			List<INamedTypeSymbol> editableViews = new List<INamedTypeSymbol>(capacity: dacFinder.GraphViews.Length);

			foreach (var viewWithType in dacFinder.GraphViews)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				if (viewWithType.Type.IsReadOnlyBqlCommand(dacFinder.PxContext))
					readOnlyViews.Add(viewWithType.Type);
				else
					editableViews.Add(viewWithType.Type);
			}

			if (editableViews.Count == 0 || dacFinder.CancellationToken.IsCancellationRequested)
				return Enumerable.Empty<ITypeSymbol>();

			return readOnlyViews.Select(viewType => viewType.GetDacFromView(dacFinder.PxContext));
		}
	}
}
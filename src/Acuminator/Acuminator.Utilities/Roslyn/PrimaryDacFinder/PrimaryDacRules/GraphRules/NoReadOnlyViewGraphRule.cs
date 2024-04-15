#nullable enable

using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to filter out views which are read-only if graph has non-read-only views.
	/// </summary>
	public class NoReadOnlyViewGraphRule(double? weight = null) : GraphRuleBase(weight)
	{
		public sealed override bool IsAbsolute => false;

		public override IEnumerable<ITypeSymbol?> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			var readOnlyViews = new List<DataViewInfo>(capacity: 4);
			var editableViews = new List<DataViewInfo>(capacity: dacFinder.GraphViews.Length);

			foreach (DataViewInfo viewInfo in dacFinder.GraphViews)
			{
				dacFinder.CancellationToken.ThrowIfCancellationRequested();

				if (viewInfo.Type.IsPXNonUpdateableBqlCommand(dacFinder.PxContext))
					readOnlyViews.Add(viewInfo);
				else
					editableViews.Add(viewInfo);
			}

			if (editableViews.Count == 0)
				return [];

			return readOnlyViews.Select(viewInfo => viewInfo.DAC);
		}
	}
}
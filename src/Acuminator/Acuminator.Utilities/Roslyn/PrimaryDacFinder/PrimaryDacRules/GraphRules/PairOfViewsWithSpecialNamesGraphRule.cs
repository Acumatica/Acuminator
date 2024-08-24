#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to add scores to the pair of views with special names in graph.
	/// </summary>
	public class PairOfViewsWithSpecialNamesGraphRule(string firstName, string secondName, double? customWeight = null) : GraphRuleBase(customWeight)
	{
		private readonly string _firstName  = firstName.CheckIfNullOrWhiteSpace();
		private readonly string _secondName = secondName.CheckIfNullOrWhiteSpace();

		public override sealed bool IsAbsolute => false;

		public override IEnumerable<ITypeSymbol?> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder.GraphSemanticModel.GraphSymbol == null || dacFinder.GraphViews.Length == 0)
				return [];

			bool firstNameFound = dacFinder.GraphSemanticModel.ViewsByNames.TryGetValue(_firstName, out DataViewInfo? firstView);
			bool secondNameFound = dacFinder.GraphSemanticModel.ViewsByNames.TryGetValue(_secondName, out DataViewInfo? secondView);

			if (!firstNameFound || !secondNameFound)
			{
				return [];
			}

			ITypeSymbol? firstDacCandidate = firstView!.Type.GetDacFromView(dacFinder.PxContext);
			ITypeSymbol? secondDacCandidate = secondView!.Type.GetDacFromView(dacFinder.PxContext);

			var dacCandidate = ChooseDacCandidate(firstDacCandidate, secondDacCandidate);
			return dacCandidate != null ? [dacCandidate] : [];
		}

		private static ITypeSymbol? ChooseDacCandidate(ITypeSymbol? firstDacCandidate, ITypeSymbol? secondDacCandidate)
		{
			if (firstDacCandidate == null && secondDacCandidate == null)
				return null;
			else if (firstDacCandidate != null && secondDacCandidate != null)
			{
				return firstDacCandidate.Equals(secondDacCandidate)
					? firstDacCandidate
					: null;
			}
			else
			{
				return firstDacCandidate ?? secondDacCandidate;
			}
		}
	}
}
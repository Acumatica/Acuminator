using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to add scores to the pair of views with special names in graph.
	/// </summary>
	public class PairOfViewsWithSpecialNamesGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly string _firstName, _secondName;

		public PairOfViewsWithSpecialNamesGraphRule(string firstName, string secondName, double? customWeight = null) : base(customWeight)
		{
			if (firstName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(firstName));
			else if (secondName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(secondName));

			_firstName = firstName;
			_secondName = secondName;
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.GraphSemanticModel?.GraphSymbol == null || dacFinder.CancellationToken.IsCancellationRequested ||
				dacFinder.GraphViews.Length == 0)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			bool firstNameFound = dacFinder.GraphSemanticModel.ViewsByNames.TryGetValue(_firstName, out var firstView);
			bool secondNameFound = dacFinder.GraphSemanticModel.ViewsByNames.TryGetValue(_secondName, out var secondView);

			if (!firstNameFound || !secondNameFound)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			ITypeSymbol firstDacCandidate = firstView.Type.GetDacFromView(dacFinder.PxContext);
			ITypeSymbol secondDacCandidate = secondView.Type.GetDacFromView(dacFinder.PxContext);

			var dacCandidate = ChooseDacCandidate(firstDacCandidate, secondDacCandidate);
			return dacCandidate?.ToEnumerable() ?? Enumerable.Empty<ITypeSymbol>();
		}

		private static ITypeSymbol ChooseDacCandidate(ITypeSymbol firstDacCandidate, ITypeSymbol secondDacCandidate)
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
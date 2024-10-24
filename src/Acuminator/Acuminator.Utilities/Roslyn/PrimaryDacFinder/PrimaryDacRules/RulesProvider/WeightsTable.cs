﻿#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ActionRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.DacRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.RulesProvider
{
	/// <summary>
	/// A table with weights for rules to determine primary DAC. The purpose of this class is to store all weights in one place for the ease of comparison and edit of weigths.
	/// </summary>
	internal class WeightsTable
	{
		public static readonly WeightsTable Default = new();

		private static readonly IImmutableDictionary<string, double> _weights = new Dictionary<string, double>
		{
			[nameof(PrimaryDacSpecifiedGraphRule)] 			   = double.MaxValue,
			[nameof(PXImportAttributeGraphRule)] 			   = 1000.0,
			[nameof(PXFilteredProcessingGraphRule)] 		   = 1000.0,

			[$"{nameof(FirstViewsInGraphRule)}-1"] 			   = 1.0,
			[$"{nameof(FirstViewsInGraphRule)}-3"] 			   = 5.0,
			[$"{nameof(FirstViewsInGraphRule)}-5"] 			   = 5.0,
			[$"{nameof(FirstViewsInGraphRule)}-10"] 		   = 10.0,

			[nameof(NoReadOnlyViewGraphRule)] 				   = -20.0,
			[nameof(ViewsWithoutPXViewNameAttributeGraphRule)] = -3.0,
			[nameof(PairOfViewsWithSpecialNamesGraphRule)] 	   = 10,

			[nameof(ForbiddenWordsInNameViewRule)] 			   = -15.0,
			[nameof(HiddenAttributesViewRule)] 				   = -50.0,
			[nameof(NoPXSetupViewRule)] 					   = -40.0,
			[nameof(PXViewNameAttributeViewRule)] 			   = 3.0,

			[nameof(ScoreSimpleActionRule)] 				   = 1.0,
			[nameof(ScoreSystemActionRule)] 				   = 8.0,

			[nameof(SameOrDescendingNamespaceDacRule)] 		   = 2.0,
		}
		.ToImmutableDictionary();

		public double this[string ruleName] => 
			ruleName.IsNullOrWhiteSpace() 
				? 0.0
				: _weights.TryGetValue(ruleName, out double weight) 
					? weight 
					: 0.0;

		private WeightsTable() { }
	}
}
#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule to filter out views which contain forbidden words.
	/// </summary>
	public class ForbiddenWordsInNameViewRule : ViewRuleBase
	{
		private readonly StringComparison _stringComparisonKind;
		private readonly List<string> _forbiddenWords;

		public override sealed bool IsAbsolute => false;

		public ForbiddenWordsInNameViewRule(bool useCaseSensitiveComparison, IEnumerable<string>? wordsToForbid = null, double? weight = null) :
									   base(weight)
		{
			_stringComparisonKind = useCaseSensitiveComparison
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			if (wordsToForbid.IsNullOrEmpty())
			{
				_forbiddenWords = GetDefaultForbiddenWords();
			}
			else
			{
				var stringComparer = useCaseSensitiveComparison 
					? StringComparer.Ordinal 
					: StringComparer.OrdinalIgnoreCase;

				_forbiddenWords = wordsToForbid.Distinct(stringComparer).ToList();
			}
		}

		/// <summary>
		/// Query if view name contains one of forbidden words.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public override sealed bool SatisfyRule(PrimaryDacFinder dacFinder, DataViewInfo viewInfo) =>
			_forbiddenWords.Any(word => viewInfo.Name.Contains(word, _stringComparisonKind));

		private List<string> GetDefaultForbiddenWords() => ["dummy"];
	}
}
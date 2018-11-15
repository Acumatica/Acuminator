using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule to filter out views which contain forbidden words.
	/// </summary>
	public class ForbiddenWordsInNameViewRule : ViewRuleBase
	{
		private readonly bool _useCaseSensitiveComparison;
		private readonly ImmutableArray<string> _forbiddenWords;

		public sealed override bool IsAbsolute => false;

		public ForbiddenWordsInNameViewRule(bool useCaseSensitiveComparison, IEnumerable<string> wordsToForbid = null, double? weight = null) :
									   base(weight)
		{
			_useCaseSensitiveComparison = useCaseSensitiveComparison;

			if (wordsToForbid.IsNullOrEmpty())
			{
				_forbiddenWords = GetDefaultForbiddenWords();
			}
			else
			{
				if (_useCaseSensitiveComparison)
				{
					wordsToForbid = wordsToForbid.Select(w => w.ToUpper());
				}

				_forbiddenWords = wordsToForbid.Distinct().ToImmutableArray();
			}
		}

		/// <summary>
		/// Query if view name contains one of forbidden words.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public sealed override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null)
				return false;

			string viewName = _useCaseSensitiveComparison
				? view.Name
				: view.Name.ToUpper();

			return _forbiddenWords.Any(word => viewName.Contains(word));
		}

		private ImmutableArray<string> GetDefaultForbiddenWords() =>
			ImmutableArray.Create("dummy");
	}
}
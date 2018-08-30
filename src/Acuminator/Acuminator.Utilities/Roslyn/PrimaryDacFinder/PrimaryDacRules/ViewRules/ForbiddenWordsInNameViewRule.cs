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
		private readonly ImmutableArray<string> forbiddenWords;

		public sealed override bool IsAbsolute => false;

		public ForbiddenWordsInNameViewRule(IEnumerable<string> wordsToForbid = null, double? weight = null) : base(weight)
		{
			forbiddenWords = wordsToForbid.IsNullOrEmpty() 
				? GetDefaultForbiddenWords()
				: wordsToForbid.ToImmutableArray();
		}
		
		/// <summary>
		/// Query if view name contains one of forbidden words.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public sealed override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType) =>
			view != null
				? forbiddenWords.Any(word => view.Name.Contains(word))
				: false;

		private ImmutableArray<string> GetDefaultForbiddenWords() =>
			ImmutableArray.Create("dummy");
	}
}
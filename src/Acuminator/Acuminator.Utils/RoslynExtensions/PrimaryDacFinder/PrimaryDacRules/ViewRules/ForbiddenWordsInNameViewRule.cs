using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Analyzers;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to filter out views which contain forbidden words.
	/// </summary>
	public class ForbiddenWordsInNameViewRule : ViewRuleBase
	{
		private readonly ImmutableArray<string> forbiddenWords;

		public sealed override bool IsAbsolute => false;

		protected override double DefaultWeight => -15;

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
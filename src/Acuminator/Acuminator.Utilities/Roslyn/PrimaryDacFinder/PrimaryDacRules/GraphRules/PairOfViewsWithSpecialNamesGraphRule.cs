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
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using PX.Data;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to add scores to the pair of views with special names in graph.
	/// </summary>
	public class PairOfViewsWithSpecialNamesGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly string firstName, secondName;

		public PairOfViewsWithSpecialNamesGraphRule(string aFirstName, string aSecondName, double? customWeight = null) : base(customWeight)
		{
			if (aFirstName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(aFirstName));
			else if (aSecondName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(aSecondName));

			firstName = aFirstName;
			secondName = aSecondName;
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.Graph == null || dacFinder.CancellationToken.IsCancellationRequested || dacFinder.GraphViewSymbolsWithTypes.Length == 0)
				return Enumerable.Empty<ITypeSymbol>();

			bool firstNameFound = false, secondNameFound = false;
			ITypeSymbol firstDacCandidate = null, secondDacCandidate = null;

			foreach (var (view, viewType) in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				if (view.Name == firstName)
				{
					firstNameFound = true;
					firstDacCandidate = viewType.GetDacFromView(dacFinder.PxContext);
					continue;
				}

				if (view.Name == secondName)
				{
					secondNameFound = true;
					secondDacCandidate = viewType.GetDacFromView(dacFinder.PxContext);
					continue;
				}

				if (firstNameFound && secondNameFound)
					break;
			}

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
				return firstDacCandidate != null
					? firstDacCandidate
					: secondDacCandidate;
			}
		}
	}
}
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


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to filter DAC based on a graph's view.
	/// </summary>
	public abstract class ViewRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.View;

		protected ViewRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Query if view satisfies this rule's conditions.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public abstract bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType);
	}
}
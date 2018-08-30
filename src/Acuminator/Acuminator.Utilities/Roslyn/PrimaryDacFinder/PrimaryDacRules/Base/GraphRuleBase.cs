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
	/// A rule to filter DAC based on a graph.
	/// </summary>
	public abstract class GraphRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.Graph;

		protected GraphRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Filter DACs from graph using this rule.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <returns/>
		public abstract IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder);
	}
}
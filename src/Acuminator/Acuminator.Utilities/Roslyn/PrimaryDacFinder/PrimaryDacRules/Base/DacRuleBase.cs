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
	/// A rule to select DAC based on rule's conditions.
	/// </summary>
	public abstract class DacRuleBase : PrimaryDacRuleBase
	{
		/// <summary>
		/// The rule kind.
		/// </summary>
		public sealed override PrimaryDacRuleKind RuleKind => PrimaryDacRuleKind.Dac;

		protected DacRuleBase(double? customWeight) : base(customWeight)
		{
		}

		/// <summary>
		/// Query if DAC satisfies this rule's conditions.
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="dac">The DAC.</param>
		/// <returns/>
		public abstract bool SatisfyRule(PrimaryDacFinder dacFinder, ITypeSymbol dac);
	}
}
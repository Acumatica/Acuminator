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
	/// A base class for primary DAC rule.
	/// </summary>
	public abstract class PrimaryDacRuleBase
	{
		/// <summary>
		/// True if this rule is absolute, if DAC has it then it is a primary DAC
		/// </summary>
		public abstract bool IsAbsolute { get; }

		/// <summary>
		/// The rule kind.
		/// </summary>
		public abstract PrimaryDacRuleKind RuleKind { get; }

		/// <summary>
		/// The weight of a trait which shows the likeliness that DAC found this rule is a primary DAC.
		/// </summary>
		public virtual double Weight { get; protected set; }

		/// <summary>
		/// The default weight value for this rule.
		/// </summary>
		protected virtual double DefaultWeight => WeightsTable.Default[this.GetType().Name];

		protected PrimaryDacRuleBase(double? customWeight)
		{
			Weight = customWeight ?? DefaultWeight;
		}
	}
}
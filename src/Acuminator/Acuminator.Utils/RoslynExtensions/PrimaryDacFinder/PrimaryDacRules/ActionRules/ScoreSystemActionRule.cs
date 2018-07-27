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
	/// A rule to  add score to DACs which has system action declared for it.
	/// </summary>
	public class ScoreSystemActionRule : ScoreSimpleActionRule
	{
		private readonly PXSystemActionsRegister systemActionsRegister;

		protected override double DefaultWeight => 4;

		public ScoreSystemActionRule(PXContext context, double? weight = null) : base(weight)
		{
			systemActionsRegister = new PXSystemActionsRegister(context);
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol action, INamedTypeSymbol actionType)
		{
			if (!base.SatisfyRule(dacFinder, action, actionType))
				return false;

			return systemActionsRegister.IsSystemAction(actionType);
		}
	}
}
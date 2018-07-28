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
	/// A rule to  add score to DACs which has action declared for it.
	/// </summary>
	public class SameOrDescendingNamespaceDacRule : DacRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public SameOrDescendingNamespaceDacRule(double? weight = null) : base(weight)
		{
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ITypeSymbol dac)
		{
			if (dacFinder?.Graph?.ContainingNamespace == null || dacFinder.CancellationToken.IsCancellationRequested ||
				dac?.ContainingNamespace == null)
			{
				return false;
			}

			var graphNameSpace = dacFinder.Graph.ContainingNamespace;
			return dac.GetContainingNamespaces().Contains(graphNameSpace);
		}
	}
}
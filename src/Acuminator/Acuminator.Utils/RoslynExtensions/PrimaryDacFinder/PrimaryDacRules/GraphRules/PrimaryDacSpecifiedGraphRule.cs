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
	/// A rule to select primary DAC when graph has primary DAC specified.
	/// </summary>
	public class PrimaryDacSpecifiedGraphRule : GraphRuleBase
	{
		public override bool IsAbsolute => true;

		protected override double DefaultWeight => double.MaxValue;

		public PrimaryDacSpecifiedGraphRule() : base(null)
		{
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder, INamedTypeSymbol graph)
		{
			if (graph == null)
				return Enumerable.Empty<ITypeSymbol>();

			ITypeSymbol primaryDac = graph.GetDeclaredPrimaryDacFromGraphOrGraphExtension(dacFinder.PxContext);
			return primaryDac?.ToEnumerable() ?? Enumerable.Empty<ITypeSymbol>();
		}
	}
}
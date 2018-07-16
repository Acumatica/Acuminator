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
	/// A rule to filter out views which are PXSetup.
	/// </summary>
	public class NoPXSetupRule : ViewRuleBase
	{
		private const double DefaultWeight = -40;
		private readonly ImmutableArray<INamedTypeSymbol> setupTypes;

		public sealed override bool IsAbsolute => false;

		public override double Weight { get; }

		public NoPXSetupRule(PXContext context, double? weight = null)
		{
			context.ThrowOnNull(nameof(context));

			setupTypes = context.BQL.GetPXSetupTypes();
			Weight = weight ?? DefaultWeight;
		}
		
		/// <summary>
		/// Query if view type is PXSetup-kind. 
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public sealed override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (dacFinder == null || viewType == null)
				return false;

			return viewType.GetBaseTypesAndThis()
						   .Any(type => setupTypes.Contains(type));
		}
	}
}
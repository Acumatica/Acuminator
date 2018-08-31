using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule to filter out views which are PXSetup.
	/// </summary>
	public class NoPXSetupViewRule : ViewRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly ImmutableArray<INamedTypeSymbol> setupTypes;

		public NoPXSetupViewRule(PXContext context, double? weight = null) : base(weight)
		{
			context.ThrowOnNull(nameof(context));

			setupTypes = context.BQL.GetPXSetupTypes();
		}
		
		/// <summary>
		/// Query if view type is PXSetup-like. 
		/// </summary>
		/// <param name="dacFinder">The DAC finder.</param>
		/// <param name="view">The view.</param>
		/// <param name="viewType">Type of the view.</param>
		/// <returns/>
		public sealed override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (dacFinder == null || viewType == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			return viewType.GetBaseTypesAndThis()
						   .Any(type => setupTypes.Contains(type));
		}
	}
}
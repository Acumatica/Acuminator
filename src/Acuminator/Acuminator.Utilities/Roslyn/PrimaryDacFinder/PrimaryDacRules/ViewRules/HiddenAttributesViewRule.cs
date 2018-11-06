using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// An excluding rule for view with PXHiddenAttribute or PXCopyPasteHiddenViewAttribute attributes.
	/// </summary>
	public class HiddenAttributesViewRule : ViewRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public HiddenAttributesViewRule(double? weight = null) : base(weight)
		{
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null || dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			ImmutableArray<AttributeData> attributes = view.GetAttributes();

			if (attributes.Length == 0)
				return false;

			INamedTypeSymbol hiddenAttribute = dacFinder.PxContext.AttributeTypes.PXHiddenAttribute;
			bool hasHiddenAttribute = attributes.Any(a => a.AttributeClass.Equals(hiddenAttribute));

			if (hasHiddenAttribute)
				return true;
			else if (dacFinder.GraphViews.Length <= 1 || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			INamedTypeSymbol copyPasteHiddenViewAttribute = dacFinder.PxContext.AttributeTypes.PXCopyPasteHiddenViewAttribute;
			return attributes.Any(a => a.AttributeClass.InheritsFromOrEquals(copyPasteHiddenViewAttribute));
		}
	}
}
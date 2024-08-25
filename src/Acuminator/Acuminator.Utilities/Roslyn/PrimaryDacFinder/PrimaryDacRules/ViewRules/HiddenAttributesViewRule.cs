#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// An excluding rule for view with PXHiddenAttribute or PXCopyPasteHiddenViewAttribute attributes.
	/// </summary>
	public class HiddenAttributesViewRule(double? weight = null) : ViewRuleBase(weight)
	{
		public override sealed bool IsAbsolute => false;

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, DataViewInfo viewInfo)
		{
			ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return false;

			INamedTypeSymbol hiddenAttribute = dacFinder.PxContext.AttributeTypes.PXHiddenAttribute;
			bool hasHiddenAttribute = attributes.Any(a => a.AttributeClass?.Equals(hiddenAttribute, SymbolEqualityComparer.Default) ?? false);

			if (hasHiddenAttribute)
				return true;
			else if (dacFinder.GraphViews.Length <= 1)
				return false;

			dacFinder.CancellationToken.ThrowIfCancellationRequested();

			INamedTypeSymbol copyPasteHiddenViewAttribute = dacFinder.PxContext.AttributeTypes.PXCopyPasteHiddenViewAttribute;
			return attributes.Any(a => a.AttributeClass?.InheritsFromOrEquals(copyPasteHiddenViewAttribute) ?? false);
		}
	}
}
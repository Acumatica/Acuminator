using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule for views with PXViewNameAttribute attribute.
	/// </summary>
	public class PXViewNameAttributeViewRule : ViewRuleBase
	{
		private readonly INamedTypeSymbol pxViewNameAttribute;

		public sealed override bool IsAbsolute => false;

		public PXViewNameAttributeViewRule(PXContext context, double? weight = null) : base(weight)
		{
			context.ThrowOnNull(nameof(context));

			pxViewNameAttribute = context.Compilation.GetTypeByMetadataName(typeof(PXViewNameAttribute).FullName);
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null || dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			ImmutableArray<AttributeData> attributes = view.GetAttributes();

			if (attributes.Length == 0)
				return false;

			return attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
							 .Any(baseType => baseType.Equals(pxViewNameAttribute));
		}
	}
}
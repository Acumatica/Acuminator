using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule for views with PXViewNameAttribute attribute.
	/// </summary>
	public class PXViewNameAttributeViewRule : ViewRuleBase
	{
		private readonly INamedTypeSymbol _pxViewNameAttribute;

		public sealed override bool IsAbsolute => false;

		public PXViewNameAttributeViewRule(PXContext context, double? weight = null) : base(weight)
		{
			context.ThrowOnNull(nameof(context));

			_pxViewNameAttribute = context.Compilation.GetTypeByMetadataName(Types.PXViewNameAttribute);
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null || dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			ImmutableArray<AttributeData> attributes = view.GetAttributes();

			if (attributes.Length == 0)
				return false;

			return attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
							 .Any(baseType => baseType.Equals(_pxViewNameAttribute));
		}
	}
}
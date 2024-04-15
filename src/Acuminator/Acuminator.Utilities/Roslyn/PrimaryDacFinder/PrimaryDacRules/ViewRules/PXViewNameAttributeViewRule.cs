#nullable enable

using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.ViewRules
{
	/// <summary>
	/// A rule for views with PXViewNameAttribute attribute.
	/// </summary>
	public class PXViewNameAttributeViewRule : ViewRuleBase
	{
		private readonly INamedTypeSymbol _pxViewNameAttribute;

		public override sealed bool IsAbsolute => false;

		public PXViewNameAttributeViewRule(PXContext context, double? weight = null) : base(weight)
		{
			_pxViewNameAttribute = context.CheckIfNull().Compilation.GetTypeByMetadataName(TypeFullNames.PXViewNameAttribute);
		}

		public override bool SatisfyRule(PrimaryDacFinder? dacFinder, DataViewInfo viewInfo)
		{
			ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return false;

			return attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
							 .Any(baseType => baseType.Equals(_pxViewNameAttribute));
		}
	}
}
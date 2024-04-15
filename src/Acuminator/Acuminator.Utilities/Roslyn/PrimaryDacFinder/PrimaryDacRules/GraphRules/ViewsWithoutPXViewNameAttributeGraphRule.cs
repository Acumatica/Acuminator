#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to penalize views without PXViewNameAttribute attribute if there are some views with PXViewNameAttribute in graph.
	/// </summary>
	public class ViewsWithoutPXViewNameAttributeGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly INamedTypeSymbol _pxViewNameAttribute;

		public ViewsWithoutPXViewNameAttributeGraphRule(PXContext context, double? customWeight = null) : base(customWeight)
		{
			_pxViewNameAttribute = context.CheckIfNull().Compilation.GetTypeByMetadataName(TypeFullNames.PXViewNameAttribute);
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder.GraphSemanticModel.GraphSymbol == null || dacFinder.GraphViews.Length == 0)
				return [];

			var dacCandidates = new List<ITypeSymbol>(dacFinder.GraphViews.Length);
			bool grapHasViewsWithViewNameAttribute = false;

			foreach (DataViewInfo viewInfo in dacFinder.GraphViews)
			{
				dacFinder.CancellationToken.ThrowIfCancellationRequested();

				ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

				if (attributes.IsDefaultOrEmpty)
					continue;

				bool viewHasViewNameAttribute = attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
														  .Any(baseType => baseType.Equals(_pxViewNameAttribute));
				if (!viewHasViewNameAttribute)
				{
					if (viewInfo.DAC != null)
						dacCandidates.Add(viewInfo.DAC);
				}
				else
				{
					grapHasViewsWithViewNameAttribute = true;
				}
			}

			return grapHasViewsWithViewNameAttribute ? dacCandidates : [];
		}
	}
}
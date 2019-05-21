using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;
using static Acuminator.Utilities.Common.Constants;

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
			context.ThrowOnNull(nameof(context));

			_pxViewNameAttribute = context.Compilation.GetTypeByMetadataName(Types.PXViewNameAttribute);
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.GraphSemanticModel?.GraphSymbol == null || dacFinder.CancellationToken.IsCancellationRequested ||
				dacFinder.GraphViews.Length == 0)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			List<ITypeSymbol> dacCandidates = new List<ITypeSymbol>(dacFinder.GraphViews.Length);
			bool grapHasViewsWithViewNameAttribute = false;

			foreach (DataViewInfo viewInfo in dacFinder.GraphViews)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

				if (attributes.Length == 0)
					continue;

				bool viewHasViewNameAttribute = attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
														  .Any(baseType => baseType.Equals(_pxViewNameAttribute));
				if (!viewHasViewNameAttribute)
				{
					var dac = viewInfo.Type.GetDacFromView(dacFinder.PxContext);

					if (dac != null)
					{
						dacCandidates.Add(dac);
					}
				}
				else
				{
					grapHasViewsWithViewNameAttribute = true;
				}
			}

			return grapHasViewsWithViewNameAttribute 
					? dacCandidates 
					: Enumerable.Empty<ITypeSymbol>();
		}
	}
}
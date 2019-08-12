using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Microsoft.CodeAnalysis;


namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to get primary DAC from PXImportAttribute cosntructor attribute if there is a view with such attribute.
	/// </summary>
	public class PXImportAttributeGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => true;

		public PXImportAttributeGraphRule(double? customWeight = null) : base(customWeight)
		{
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return Enumerable.Empty<ITypeSymbol>();

			List<ITypeSymbol> primaryDacCandidates = new List<ITypeSymbol>(1);

			foreach (DataViewInfo viewInfo in dacFinder.GraphViews)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

				if (attributes.Length == 0)
					continue;

				var importAttributeType = dacFinder.PxContext.AttributeTypes.PXImportAttribute;
				var importAttributeData = attributes.FirstOrDefault(a => a.AttributeClass.Equals(importAttributeType));

				if (importAttributeData == null)
					continue;
				else if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				var dacArgType = (from arg in importAttributeData.ConstructorArguments
								  where arg.Kind == TypedConstantKind.Type && arg.Type.IsDAC()
								  select arg.Type)
								 .FirstOrDefault();

				if (dacArgType != null)
				{
					primaryDacCandidates.Add(dacArgType);
				}
			}

			return primaryDacCandidates;
		}
	}
}
#nullable enable

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
	/// A rule to get primary DAC from PXImportAttribute constructor if there is a view with such attribute.
	/// </summary>
	public class PXImportAttributeGraphRule(double? customWeight = null) : GraphRuleBase(customWeight)
	{
		public override sealed bool IsAbsolute => true;

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			var primaryDacCandidates = new List<ITypeSymbol>(1);

			foreach (DataViewInfo viewInfo in dacFinder.GraphViews)
			{
				dacFinder.CancellationToken.ThrowIfCancellationRequested();

				ImmutableArray<AttributeData> attributes = viewInfo.Symbol.GetAttributes();

				if (attributes.IsDefaultOrEmpty)
					continue;

				var importAttributeType = dacFinder.PxContext.AttributeTypes.PXImportAttribute;
				var importAttributeData = 
					attributes.FirstOrDefault(a => a.AttributeClass?.Equals(importAttributeType, SymbolEqualityComparer.Default) ?? false);

				if (importAttributeData == null)
					continue;
				
				var dacArgType = (from arg in importAttributeData.ConstructorArguments
								  where arg.Kind == TypedConstantKind.Type && arg.Type != null && arg.Type.IsDAC(dacFinder.PxContext)
								  select arg.Type)
								 .FirstOrDefault();

				if (dacArgType != null)
					primaryDacCandidates.Add(dacArgType);
			}

			return primaryDacCandidates.Count <= 1
				? primaryDacCandidates
				: ResolveMultipleDacCandidatesFomDifferentViews(primaryDacCandidates);
		}

		private IEnumerable<ITypeSymbol> ResolveMultipleDacCandidatesFomDifferentViews(List<ITypeSymbol> primaryDacCandidates)
		{
			var distinctDacCandidates = primaryDacCandidates.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default).ToList();

			return distinctDacCandidates.Count <= 1
				? distinctDacCandidates
				: [];
		}
	}
}
#nullable enable

using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to determine primary DAC from PXFilteredProcessingType view's second type parameter.
	/// </summary>
	public class PXFilteredProcessingGraphRule : GraphRuleBase
	{
		private readonly ITypeSymbol? _pxFilteredProcessingType;

		public sealed override bool IsAbsolute => true;

		public PXFilteredProcessingGraphRule(PXContext context, double? customWeight = null) : base(customWeight)
		{
			_pxFilteredProcessingType = context.CheckIfNull().Compilation.GetTypeByMetadataName(TypeFullNames.PXFilteredProcessing);
		}

		public override IEnumerable<ITypeSymbol?> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (_pxFilteredProcessingType == null || dacFinder.GraphSemanticModel.GraphSymbol == null || 
				dacFinder.GraphViews.Length == 0)
			{
				return [];
			}

			var primaryDacCandidates = new List<ITypeSymbol>();

			foreach (DataViewInfo view in dacFinder.GraphViews)
			{
				dacFinder.CancellationToken.ThrowIfCancellationRequested();

				var fProcessingView = 
					view.Type.GetBaseTypesAndThis()
							 .FirstOrDefault(t => _pxFilteredProcessingType.Equals(t, SymbolEqualityComparer.Default) || 
												  _pxFilteredProcessingType.Equals(t.OriginalDefinition, SymbolEqualityComparer.Default));

				if (fProcessingView is not INamedTypeSymbol filteredProcessingView)
					continue;

				var typeParameters = filteredProcessingView.TypeArguments;

				if (typeParameters.Length < 2 || !typeParameters[1].IsDAC())
					continue;

				primaryDacCandidates.Add(typeParameters[1]);
			}

			return primaryDacCandidates;
		}
	}
}
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to determine primary DAC from PXFilteredProcessingType view's second type parameter.
	/// </summary>
	public class PXFilteredProcessingGraphRule : GraphRuleBase
	{
		private readonly ITypeSymbol _pxFilteredProcessingType;

		public sealed override bool IsAbsolute => true;

		public PXFilteredProcessingGraphRule(PXContext context, double? customWeight = null) : base(customWeight)
		{
			context.ThrowOnNull(nameof(context));

			_pxFilteredProcessingType = context.Compilation.GetTypeByMetadataName(TypeFullNames.PXFilteredProcessing);
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (_pxFilteredProcessingType == null || dacFinder?.GraphSemanticModel?.GraphSymbol == null || 
				dacFinder.CancellationToken.IsCancellationRequested || dacFinder.GraphViews.Length == 0)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			List<ITypeSymbol> primaryDacCandidates = new List<ITypeSymbol>(1);

			foreach (DataViewInfo view in dacFinder.GraphViews)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				var fProcessingView = view.Type.GetBaseTypesAndThis()
											   .FirstOrDefault(t => _pxFilteredProcessingType.Equals(t) || 
																    _pxFilteredProcessingType.Equals(t?.OriginalDefinition));

				if (fProcessingView == null || !(fProcessingView is INamedTypeSymbol filteredProcessingView))
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
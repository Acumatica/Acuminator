using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using PX.Data;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to determine primary DAC from PXFilteredProcessingType view's second type parameter.
	/// </summary>
	public class PXFilteredProcessingGraphRule : GraphRuleBase
	{
		private readonly ITypeSymbol pxFilteredProcessingType;

		public sealed override bool IsAbsolute => true;

		public PXFilteredProcessingGraphRule(PXContext context, double? customWeight = null) : base(customWeight)
		{
			context.ThrowOnNull(nameof(context));

			pxFilteredProcessingType = context.Compilation.GetTypeByMetadataName(typeof(PXFilteredProcessing<,>).FullName);
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (pxFilteredProcessingType == null ||
				dacFinder?.Graph == null || dacFinder.CancellationToken.IsCancellationRequested || dacFinder.GraphViewSymbolsWithTypes.Length == 0)
			{
				return Enumerable.Empty<ITypeSymbol>();
			}

			List<ITypeSymbol> primaryDacCandidates = new List<ITypeSymbol>(1);

			foreach (var (view, viewType) in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				var fProcessingView = viewType.GetBaseTypesAndThis()
											  .FirstOrDefault(t => pxFilteredProcessingType.Equals(t) || 
																   pxFilteredProcessingType.Equals(t?.OriginalDefinition));

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
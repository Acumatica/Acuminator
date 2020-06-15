using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	/// <summary>
	/// Base class for DAC key declaration analyzers which provides filtering of the DACs for the diagnostics.
	/// </summary>
	public abstract class DacKeyDeclarationAnalyzerBase : DacAggregatedAnalyzerBase
	{
		public override bool ShouldAnalyze(PXContext context, DacSemanticModel dac) =>
			base.ShouldAnalyze(context, dac) &&
			dac.DacType == DacType.Dac && !dac.IsMappedCacheExtension && !dac.Symbol.IsAbstract &&
			context.ReferentialIntegritySymbols.AreDefined && ShouldAnalyzeDac(context, dac);

		private bool ShouldAnalyzeDac(PXContext context, DacSemanticModel dac)
		{
			if (dac.IsFullyUnbound())
				return false;

			var dacAttributes = dac.Symbol.GetAttributes();

			if (dacAttributes.IsDefaultOrEmpty)
				return false;

			var pxCacheNameAttribute = context.AttributeTypes.PXCacheNameAttribute;
			var pxPrimaryGraphAttribute = context.AttributeTypes.PXPrimaryGraphAttribute;

			return dacAttributes.Any(attribute => attribute.AttributeClass != null && 
												 (attribute.AttributeClass.InheritsFromOrEquals(pxCacheNameAttribute) ||
												  attribute.AttributeClass.InheritsFromOrEquals(pxPrimaryGraphAttribute)));
		}
	}
}
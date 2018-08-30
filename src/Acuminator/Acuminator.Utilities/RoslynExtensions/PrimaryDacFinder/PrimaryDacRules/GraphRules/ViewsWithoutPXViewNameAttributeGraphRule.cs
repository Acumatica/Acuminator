using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Analyzers;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using PX.Data;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to penalize views without PXViewNameAttribute attribute if there are some views with PXViewNameAttribute in graph.
	/// </summary>
	public class ViewsWithoutPXViewNameAttributeGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly INamedTypeSymbol pxViewNameAttribute;

		public ViewsWithoutPXViewNameAttributeGraphRule(PXContext context, double? customWeight = null) : base(customWeight)
		{
			context.ThrowOnNull(nameof(context));

			pxViewNameAttribute = context.Compilation.GetTypeByMetadataName(typeof(PXViewNameAttribute).FullName);
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.Graph == null || dacFinder.CancellationToken.IsCancellationRequested || dacFinder.GraphViewSymbolsWithTypes.Length == 0)
				return Enumerable.Empty<ITypeSymbol>();

			List<ITypeSymbol> dacCandidates = new List<ITypeSymbol>(dacFinder.GraphViewSymbolsWithTypes.Length);
			bool grapHasViewsWithViewNameAttribute = false;

			foreach (var (view, viewType) in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				ImmutableArray<AttributeData> attributes = view.GetAttributes();

				if (attributes.Length == 0)
					continue;

				bool viewHasViewNameAttribute = attributes.SelectMany(a => a.AttributeClass.GetBaseTypesAndThis())
														  .Any(baseType => baseType.Equals(pxViewNameAttribute));
				if (!viewHasViewNameAttribute)
				{
					var dac = viewType.GetDacFromView(dacFinder.PxContext);

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
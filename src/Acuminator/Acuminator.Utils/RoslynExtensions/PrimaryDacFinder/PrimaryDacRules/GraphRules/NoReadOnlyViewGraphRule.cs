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


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// A rule to filter out views which are read only if graph has non read only views.
	/// </summary>
	public class NoReadOnlyViewGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public NoReadOnlyViewGraphRule(double? weight = null) : base(weight)
		{
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return Enumerable.Empty<ITypeSymbol>();

			List<INamedTypeSymbol> readOnlyViews = new List<INamedTypeSymbol>(capacity: 4);
			List<INamedTypeSymbol> editableViews = new List<INamedTypeSymbol>(capacity: dacFinder.GraphViewSymbolsWithTypes.Length);

			foreach (var viewWithType in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				if (viewWithType.ViewType.IsReadOnlyBqlCommand(dacFinder.PxContext))
					readOnlyViews.Add(viewWithType.ViewType);
				else
					editableViews.Add(viewWithType.ViewType);
			}

			if (editableViews.Count == 0 || dacFinder.CancellationToken.IsCancellationRequested)
				return Enumerable.Empty<ITypeSymbol>();

			return readOnlyViews.Select(viewType => viewType.GetDacFromView(dacFinder.PxContext));
		}
	}
}
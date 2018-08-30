using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;


namespace Acuminator.Utilities.PrimaryDAC
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

			foreach (var (view, viewType) in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				ImmutableArray<AttributeData> attributes = view.GetAttributes();

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
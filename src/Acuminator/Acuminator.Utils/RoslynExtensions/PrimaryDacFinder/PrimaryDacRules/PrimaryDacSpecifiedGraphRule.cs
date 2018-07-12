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
	/// A rule to select primary DAC when graph has primary DAC specified.
	/// </summary>
	public class PrimaryDacSpecifiedGraphRule : GraphRuleBase
	{
		public override bool IsAbsolute => true;

		public override double Weight => double.MaxValue;

		public override IEnumerable<ITypeSymbol> FindPrimaryDacCandidatesByRule(PrimaryDacFinder dacFinder, INamedTypeSymbol graph, 
																				ClassDeclarationSyntax graphNode)
		{
			if (graph == null)
				return Enumerable.Empty<ITypeSymbol>();

			var baseGraphType = graph.GetBaseTypesAndThis()
									 .OfType<INamedTypeSymbol>()
									 .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type));

			return baseGraphType != null 
				? baseGraphType.TypeArguments[1].ToEnumerable()
				: Enumerable.Empty<ITypeSymbol>();
		}

		private static bool IsGraphWithPrimaryDacBaseGenericType(INamedTypeSymbol type) =>
			type.TypeArguments.Length >= 2 && type.Name.Equals(TypeNames.PXGraph, StringComparison.Ordinal);	
	}
}
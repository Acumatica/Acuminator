using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.PXGraphCreationForBqlQueries
{
	public class InstanceUsageWalker : CSharpSyntaxWalker
	{
		private readonly ImmutableHashSet<ISymbol> _symbols;
		private readonly SemanticModel _semanticModel;

		public InstanceUsageWalker(IEnumerable<ISymbol> symbols, SemanticModel semanticModel)
		{
			_symbols = symbols.ToImmutableHashSet();
			_semanticModel = semanticModel;
		}

		public override void VisitIdentifierName(IdentifierNameSyntax node)
		{
			var symbol = _semanticModel.GetSymbolInfo(node).Symbol;

			if (symbol != null && _symbols.Contains(symbol))
			{

			}
		}
	}
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public class MultipleParametersUsagesRewriter : CSharpSyntaxRewriter
	{
		private readonly IDictionary<IParameterSymbol, SyntaxNode> _parametersWithReplacements;
		private readonly SemanticModel _semanticModel;

		private readonly CancellationToken _cancellation;

		public MultipleParametersUsagesRewriter(IDictionary<IParameterSymbol, SyntaxNode> parametersWithReplacements, SemanticModel semanticModel,
												CancellationToken cancellation)
		{
			_parametersWithReplacements = parametersWithReplacements.CheckIfNull();
			_semanticModel 				= semanticModel.CheckIfNull();
			_cancellation  				= cancellation;
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			_cancellation.ThrowIfCancellationRequested();

			var symbolInfo = _semanticModel.GetSymbolOrFirstCandidate(node, _cancellation) as IParameterSymbol;

			if (symbolInfo == null || !_parametersWithReplacements.TryGetValue(symbolInfo, out SyntaxNode replaceWith))
				return base.VisitIdentifierName(node);

			var replacementNode = replaceWith;

			if (node.HasLeadingTrivia)
				replacementNode = replacementNode.WithLeadingTrivia(node.GetLeadingTrivia());

			if (node.HasTrailingTrivia)
				replacementNode = replacementNode.WithTrailingTrivia(node.GetTrailingTrivia());

			return replacementNode;
		}
	}
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Syntax
{
	public class ParameterUsagesRewriter : CSharpSyntaxRewriter
	{
		private readonly IParameterSymbol _parameter;
		private readonly SyntaxNode _replaceWith;
		private readonly SemanticModel _semanticModel;

		private readonly CancellationToken _cancellation;

		public ParameterUsagesRewriter(IParameterSymbol parameter, SyntaxNode replaceWith, SemanticModel semanticModel, CancellationToken cancellation)
		{
			_parameter = parameter.CheckIfNull();
			_replaceWith = replaceWith.CheckIfNull();
			_semanticModel = semanticModel.CheckIfNull();
			_cancellation = cancellation;
		}

		public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
		{
			_cancellation.ThrowIfCancellationRequested();

			var symbolInfo = _semanticModel.GetSymbolInfo(node, _cancellation);

			if (symbolInfo.Symbol != null && symbolInfo.Symbol.Equals(_parameter))
			{
				var replacement = _replaceWith;

				if (node.HasLeadingTrivia)
					replacement = replacement.WithLeadingTrivia(node.GetLeadingTrivia());

				if (node.HasTrailingTrivia)
					replacement = replacement.WithTrailingTrivia(node.GetTrailingTrivia());

				return replacement;
			}

			return base.VisitIdentifierName(node);
		}
	}
}

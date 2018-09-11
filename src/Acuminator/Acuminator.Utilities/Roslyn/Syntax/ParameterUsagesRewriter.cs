using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

		public ParameterUsagesRewriter(IParameterSymbol parameter, SyntaxNode replaceWith,
			SemanticModel semanticModel)
		{
			parameter.ThrowOnNull(nameof(parameter));
			replaceWith.ThrowOnNull(nameof(replaceWith));
			semanticModel.ThrowOnNull(nameof(semanticModel));

			_parameter = parameter;
			_replaceWith = replaceWith;
			_semanticModel = semanticModel;
		}

		public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
		{
			var symbolInfo = _semanticModel.GetSymbolInfo(node);

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

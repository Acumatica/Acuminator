using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PX.Analyzers.Utilities;

namespace PX.Analyzers.Vsix.Formatter
{
	class BqlStatementRewriter : BqlRewriterBase
	{
		public BqlStatementRewriter(BqlContext context, SemanticModel semanticModel, 
			SyntaxTriviaList endOfLineTrivia, SyntaxTriviaList indentationTrivia, SyntaxTriviaList defaultLeadingTrivia) 
			: base(context, semanticModel, endOfLineTrivia, indentationTrivia, defaultLeadingTrivia)
		{
		}

		public BqlStatementRewriter(BqlRewriterBase parent, SyntaxTriviaList defaultLeadingTrivia) 
			: base(parent, defaultLeadingTrivia)
		{
		}


		public override SyntaxNode VisitGenericName(GenericNameSyntax node)
		{
			INamedTypeSymbol typeSymbol = GetTypeSymbol(node);
			INamedTypeSymbol constructedFromSymbol = typeSymbol?.ConstructedFrom; // get generic type

			if (constructedFromSymbol != null)
			{
				// Move first table declaration to a new line and indent it
				if (constructedFromSymbol.InheritsFromOrEquals(Context.PXSelectBase)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlSelect)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
				{
					var childRewriter = new BqlFirstTableRewriter(this, DefaultLeadingTrivia);
					node = (GenericNameSyntax) childRewriter.Visit(node);
				}

				if (constructedFromSymbol.ImplementsInterface(Context.IBqlWhere)
				    || constructedFromSymbol.ImplementsInterface(Context.IBqlOrderBy))
				{
					var newNode = OnNewLineAndIndented(node);
					var childRewriter = new BqlStatementRewriter(this, IndentedDefaultTrivia);
					return newNode.WithTypeArgumentList((TypeArgumentListSyntax) childRewriter.Visit(newNode.TypeArgumentList));
				}

				if (constructedFromSymbol.ImplementsInterface(Context.IBqlJoin)
					|| constructedFromSymbol.ImplementsInterface(Context.IBqlSortColumn))
				{
					var newNode = OnNewLineAndIndented(node);
					var childRewriter = new BqlStatementRewriter(this, DefaultLeadingTrivia);
					return newNode.WithTypeArgumentList((TypeArgumentListSyntax) childRewriter.Visit(newNode.TypeArgumentList));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}

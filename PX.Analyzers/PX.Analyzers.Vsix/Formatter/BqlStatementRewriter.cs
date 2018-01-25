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
			INamedTypeSymbol originalSymbol = GetOriginalTypeSymbol(node);

			if (originalSymbol != null)
			{
				// Move first table declaration to a new line and indent it
				if (originalSymbol.InheritsFromOrEqualsGeneric(Context.PXSelectBase)
				    || originalSymbol.ImplementsInterface(Context.IBqlSelect)
				    || originalSymbol.ImplementsInterface(Context.IBqlSearch)) // TODO: could be Coalesce - handle this case in the future
				{
					var childRewriter = new BqlFirstTableRewriter(this, DefaultLeadingTrivia);
					node = (GenericNameSyntax) childRewriter.Visit(node);
				}

				// Each time we see one of this statements, increase indent and move statement to new line
				if (originalSymbol.ImplementsInterface(Context.IBqlOrderBy))
				{
					return RewriteGenericNode(node, new BqlStatementRewriter(this, IndentedDefaultTrivia));
				}

				// Each time we see one of this statements, move statement to new line
				if (originalSymbol.ImplementsInterface(Context.IBqlSortColumn))
				{
					return RewriteGenericNode(node, new BqlStatementRewriter(this, DefaultLeadingTrivia));
				}

				if (originalSymbol.ImplementsInterface(Context.IBqlJoin))
				{
					var rewriter = new BqlJoinRewriter(this, DefaultLeadingTrivia);
					return rewriter.Visit(node);
				}

				if (originalSymbol.ImplementsInterface(Context.IBqlWhere))
				{
					return RewriteGenericNode(node, new BqlConditionRewriter(this, IndentedDefaultTrivia));
				}

				if (originalSymbol.InheritsFromOrEqualsGeneric(Context.Aggregate))
				{
					return RewriteGenericNode(node, new BqlAggregateRewriter(this, IndentedDefaultTrivia));
				}
			}

			return base.VisitGenericName(node);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers.Analyzers.BQL
{
	public static class BqlRoslynUtil
	{
        /// <summary>
        /// An ITypeSymbol extension method that query if 'typeSymbol' is bql command.
        /// </summary>
        /// <param name="typeSymbol">The typeSymbol to act on.</param>
        /// <param name="context">The context.</param>
        /// <returns>
        /// True if bql command, false if not.
        /// </returns>
        public static bool IsBqlCommand(this ITypeSymbol typeSymbol, PXContext context)
        {
            if (typeSymbol == null)
                return false;

            if (typeSymbol.InheritsFrom(context.PXSelectBaseType))
                return true;

            return false;
        }

        public static bool EndsTheLine(this SyntaxNode node) => node.GetTrailingTrivia().TriviaContainsEOL();		

		public static bool StartTheLine(this SyntaxNode node) => node.GetTrailingTrivia().TriviaContainsEOL();

		public static bool HasEmptyLineAfter(this SyntaxNode node) => node.GetTrailingTrivia().TriviaHasEmptyLine();

		public static bool HasEmptyLineBefore(this SyntaxNode node) => node.GetLeadingTrivia().TriviaHasEmptyLine();

		public static bool HasExactlyOneEOL(this SyntaxNode node) => node.GetTrailingTrivia().HasExactlyOneEOL();

		public static bool HasExactlyOneEOL(this SyntaxToken token) => token.TrailingTrivia.HasExactlyOneEOL();

		public static bool TriviaContainsEOL(this SyntaxTriviaList triviaList) => 
			triviaList.Any(trivia => trivia.Kind() == SyntaxKind.EndOfLineTrivia);

		public static bool TriviaHasEmptyLine(this SyntaxTriviaList triviaList)
		{
			int eolCount = 0;

			foreach (var trivia in triviaList.Where(tr => tr.Kind() == SyntaxKind.EndOfLineTrivia))
			{
				eolCount++;

				if (eolCount > 1)
					return true;
			}

			return false;
		}

		public static bool HasExactlyOneEOL(this SyntaxTriviaList triviaList)
		{
			int eolCount = 0;

			foreach (var trivia in triviaList.Where(tr => tr.Kind() == SyntaxKind.EndOfLineTrivia))
			{
				eolCount++;

				if (eolCount > 1)
					return false;
			}

			return eolCount == 1;
		}


		public static bool CheckGenericNodeParentKind(this GenericNameSyntax genericNode)
		{
			if (genericNode?.Parent == null)
				return false;

			SyntaxKind parentKind = genericNode.Parent.Kind();

			if (parentKind == SyntaxKind.VariableDeclaration)
				return true;

			if (parentKind == SyntaxKind.SimpleMemberAccessExpression)
			{
				SyntaxKind? grandPaKind = genericNode.Parent.Parent?.Kind();

				if (grandPaKind == SyntaxKind.InvocationExpression)
					return true;
			}


			return false;
		}
	}
}

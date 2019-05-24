using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Microsoft.VisualStudio.Text.Editor;

using Enumerable = System.Linq.Enumerable;


namespace Acuminator.Vsix.Formatter
{
	public class BqlFormatter
	{
		private readonly SyntaxTriviaList EndOfLineTrivia;
		private readonly SyntaxTriviaList IndentationTrivia;

		public BqlFormatter(string endOfLineCharacter, bool useTabs, int tabSize, int indentSize)
		{
			EndOfLineTrivia = SyntaxTriviaList.Create(SyntaxFactory.EndOfLine(endOfLineCharacter));

			if (useTabs && indentSize >= tabSize)
			{
				var indentItems = GetUseTabsModeIndentTrivias(indentSize, tabSize);
			    IndentationTrivia = indentItems.ToSyntaxTriviaList();
			}
			else
			{
				IndentationTrivia = SyntaxTriviaList.Create(SyntaxFactory.Whitespace(new string(' ', indentSize)));
			}
		}

		public SyntaxNode Format(SyntaxNode syntaxRoot, SemanticModel semanticModel)
		{
			 var rewriter = new BqlRewriter(new BqlContext(semanticModel.Compilation), semanticModel,
				EndOfLineTrivia, IndentationTrivia);
			return rewriter.Visit(syntaxRoot);
		}

		public static BqlFormatter FromTextView(IWpfTextView textView)
		{
			textView.ThrowOnNull(nameof(textView));

			int indentSize = textView.Options.GetOptionValue(DefaultOptions.IndentSizeOptionId);
			int tabSize = textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);
			bool convertTabsToSpaces = textView.Options.GetOptionValue(DefaultOptions.ConvertTabsToSpacesOptionId);
			string newLineCharacter = textView.Options.GetOptionValue(DefaultOptions.NewLineCharacterOptionId);

			return new BqlFormatter(newLineCharacter, !convertTabsToSpaces, tabSize, indentSize);
		}

		private static IEnumerable<SyntaxTrivia> GetUseTabsModeIndentTrivias(int indentSize, int tabSize)
		{
			foreach (SyntaxTrivia indentTabTrivia in Enumerable.Repeat(SyntaxFactory.Tab, indentSize / tabSize))
			{
				yield return indentTabTrivia;
			}

			int remainingSpaceTrivia = indentSize % tabSize;

			if (remainingSpaceTrivia != 0)
			{
				yield return SyntaxFactory.Whitespace(new string(' ', remainingSpaceTrivia));
			}
		}
	}
}

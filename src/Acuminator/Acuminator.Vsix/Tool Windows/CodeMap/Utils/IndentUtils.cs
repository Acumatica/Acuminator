using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public static class IndentUtils
	{
		private const string PxDataNamespacePrefix = "PX.Data.";
		private const string PxObjectsNamespacePrefix = "PX.Objects.";

		public static int GetPrependLength(SyntaxTokenList? modifiers) => modifiers != null
			? modifiers.Value.FullSpan.End - modifiers.Value.Span.Start
			: 0;

		public static string GetSyntaxNodeStringWithRemovedIndent(this SyntaxNode syntaxNode, int tabSize, int prependLength = 0)
		{
			if (tabSize <= 0)
			{
				throw new ArgumentException("Tab size must be positive", nameof(tabSize));
			}

			string syntaxNodeString = syntaxNode?.ToString();

			if (syntaxNodeString.IsNullOrWhiteSpace())
				return syntaxNodeString;

			prependLength = tabSize * (prependLength / tabSize);
			var indentLength = prependLength + syntaxNode.GetNodeIndentLength(tabSize);

			if (indentLength == 0)
				return syntaxNodeString;

			var sb = new System.Text.StringBuilder(string.Empty, capacity: syntaxNodeString.Length);
			int counter = 0;
			bool isInsideString = false;
			bool codeStarted = false;
			char prevChar = default;

			foreach (char c in syntaxNodeString)
			{
				ProcessCurrentCharacter(c, prevChar);

				prevChar = c;
			}

			return sb.ToString();

			//-----------------------------------------------Local Function-------------------------------------
			void ProcessCurrentCharacter(char c, char prevCharacter)
			{
				switch (c)
				{
					case '"':
						ProcessQuotationCharacter(prevCharacter);
						sb.Append(c);
						return;

					case '\n':
						counter = 0;
						codeStarted = false;
						sb.Append(c);
						return;

					case ' ' when counter < indentLength && !isInsideString && !codeStarted: //-V3063
						counter++;
						return;

					case '\t' when counter < indentLength && !isInsideString && !codeStarted: //-V3063
						counter += tabSize;
						return;

					case ' ' when counter >= indentLength || isInsideString || codeStarted:
					case '\t' when counter >= indentLength || isInsideString || codeStarted:
						sb.Append(c);
						return;

					default:
						codeStarted = true;
						sb.Append(c);
						return;
				}
			}

			void ProcessQuotationCharacter(char prevCharacter)
			{
				switch (prevCharacter)
				{
					case '\\':
						return;

					default:
						isInsideString = !isInsideString;
						return;
				}
			}
		}

	
		public static string RemoveCommonAcumaticaNamespacePrefixes(this string codeFragment) =>
			codeFragment?.Replace(PxDataNamespacePrefix, string.Empty)
						?.Replace(PxObjectsNamespacePrefix, string.Empty);

		public static int GetNodeIndentLength(this SyntaxNode node, int tabSize)
		{
			if (tabSize <= 0)
			{
				throw new ArgumentException("Tab size must be positive", nameof(tabSize));
			}

			if (node == null)
				return 0;

			SyntaxNode currentNode = node;

			while (currentNode != null)
			{
				var leadingTrivia = currentNode.GetLeadingTrivia();
				
				if (leadingTrivia.Count > 0)
				{
					int indentLength = leadingTrivia.LastWhitespaceTrivia()
												   ?.GetIndentLengthFromWhitespaceTrivia(tabSize) ?? 0;
					if (indentLength > 0)
						return indentLength;
				}

				currentNode = currentNode.Parent;
			}

			return 0;
		}

		private static SyntaxTrivia? LastWhitespaceTrivia(this SyntaxTriviaList syntaxTrivias)
		{
			for (int i = syntaxTrivias.Count - 1; i >= 0; i--)
			{
				SyntaxTrivia trivia = syntaxTrivias[i];

				if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
					return trivia;
			}

			return null;
		}

		private static int GetIndentLengthFromWhitespaceTrivia(this SyntaxTrivia trivia, int tabSize)
		{
			string triviaText = trivia.ToString();
			int indentLength = 0;

			foreach (char c in triviaText)
			{
				switch (c)
				{
					case ' ':
						indentLength++;
						continue;
					case '\t':
						indentLength += tabSize;
						continue;
				}
			}

			return indentLength;
		}	
	}
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public static class IndentUtils
	{
		private const string PxDataNamespacePrefix = "PX.Data.";
		private const string PxObjectsNamespacePrefix = "PX.Objects.";

		public static string GetSyntaxNodeStringWithRemovedIndent(this SyntaxNode syntaxNode, int tabSize)
		{
			if (tabSize <= 0)
			{
				throw new ArgumentException("Tab size must be positive", nameof(tabSize));
			}

			string syntaxNodeString = syntaxNode?.ToString();

			if (syntaxNodeString.IsNullOrWhiteSpace())
				return syntaxNodeString;

			var indentLength = syntaxNode.GetNodeIndentLength(tabSize);

			if (indentLength == 0)
				return syntaxNodeString;

			var sb = new System.Text.StringBuilder(string.Empty, capacity: syntaxNodeString.Length);
			int counter = 0;

			foreach (char c in syntaxNodeString)
			{
				switch (c)
				{
					case '\n':
						counter = 0;
						sb.Append(c);
						continue;

					case ' ' when counter < indentLength:
						counter++;
						continue;

					case '\t' when counter < indentLength:
						counter += tabSize;
						continue;

					case ' ' when counter >= indentLength:
					case '\t' when counter >= indentLength:
					default:
						sb.Append(c);
						continue;
				}
			}

			return sb.ToString();
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

			int indentLength = 0;
			SyntaxNode currentNode = node;

			while (currentNode != null)
			{
				var leadingTrivia = currentNode.GetLeadingTrivia();
				
				if (leadingTrivia.Count > 0)
				{
					indentLength += leadingTrivia.Where(t => t.IsKind(SyntaxKind.WhitespaceTrivia))
												 .Sum(t => t.GetIndentLengthFromWhitespaceTrivia(tabSize));
				}

				currentNode = currentNode.Parent;
			}

			return indentLength;
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

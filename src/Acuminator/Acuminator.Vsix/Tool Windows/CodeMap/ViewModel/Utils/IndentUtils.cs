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

		public static int GetNodeIndentLevel(this SyntaxNode node)
		{
			if (node == null)
				return 0;

			int indentLevel = 0;

			SyntaxNode currentNode = node;

			while (currentNode != null)
			{
				var leadingTrivia = currentNode.GetLeadingTrivia();

				if (leadingTrivia.Count > 0)
				{
					indentLevel += leadingTrivia.Count(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
				}

				currentNode = currentNode.Parent;
			}

			return indentLevel;
		}

		public static string GetAttributeListStringWithRemovedIndent(this AttributeListSyntax attributeList)
		{
			string attributeListString = attributeList?.ToString();

			if (attributeListString.IsNullOrWhiteSpace())
				return attributeListString;
			
			var triviaCount = attributeList.GetNodeIndentLevel();

			if (triviaCount == 0)
				return attributeListString;

			var sb = new System.Text.StringBuilder(string.Empty, capacity: attributeListString.Length);
			int counter = 0;

			for (int i = 0; i < attributeListString.Length; i++)
			{
				char c = attributeListString[i];

				switch (c)
				{
					case '\n':
						counter = 0;
						sb.Append(c);
						continue;
					case '\t' when counter < triviaCount:
						counter++;
						continue;
					case '\t' when counter >= triviaCount:
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
	}
}

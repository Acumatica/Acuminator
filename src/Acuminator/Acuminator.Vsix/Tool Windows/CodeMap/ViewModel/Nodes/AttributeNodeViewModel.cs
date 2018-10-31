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
	public class AttributeNodeViewModel : TreeNodeViewModel
	{
		private const string AttributeSuffix = nameof(System.Attribute);
		public AttributeData Attribute { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public string Tooltip { get; }

		public AttributeNodeViewModel(GraphMemberNodeViewModel graphMemberVM, AttributeData attribute, 
									  bool isExpanded = false) :
								 base(graphMemberVM?.Tree, isExpanded)
		{
			attribute.ThrowOnNull(nameof(attribute));

			Attribute = attribute;
			int lastDotIndex = Attribute.AttributeClass.Name.LastIndexOf('.');
			string attributeName = lastDotIndex >= 0 && lastDotIndex < Attribute.AttributeClass.Name.Length - 1
				? Attribute.AttributeClass.Name.Substring(lastDotIndex + 1)
				: Attribute.AttributeClass.Name;

			int lastAttributeSuffixIndex = attributeName.LastIndexOf(AttributeSuffix);
			bool endsWithSuffix = attributeName.Length == (lastAttributeSuffixIndex + AttributeSuffix.Length);

			if (lastAttributeSuffixIndex > 0 && endsWithSuffix)
			{
				attributeName = attributeName.Remove(lastAttributeSuffixIndex);
			}

			Name = $"[{attributeName}]";
			var cancellationToken = Tree.CodeMapViewModel.CancellationToken.GetValueOrDefault();
			var attributeNode = Attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.Parent as AttributeListSyntax;
			Tooltip = attributeNode?.ToString().TrimIndent(attributeNode) ?? $"[{Attribute.ToString()}]";
		}

		public override void NavigateToItem()
		{
			if (Attribute.ApplicationSyntaxReference?.SyntaxTree == null)
				return;

			TextSpan span = Attribute.ApplicationSyntaxReference.Span;
			string filePath =  Attribute.ApplicationSyntaxReference.SyntaxTree.FilePath;
			Workspace workspace = AcuminatorVSPackage.Instance.GetVSWorkspace();

			if (workspace?.CurrentSolution == null)
				return;

			AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPosition(workspace.CurrentSolution, 
																			filePath, span.Start);
		}
	}

	public static class IndentExt
	{
		public static int GetAttributeIndentLevel(this AttributeListSyntax node)
		{
			if (node == null)
				return 0;

			int indentLevel = 0;

			SyntaxNode current = node;

			while (current != null)
			{
				if (current.HasLeadingTrivia)
				{
					indentLevel += node.GetLeadingTrivia()
									   .Count(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
				}

				current = current.Parent;
			}

			return indentLevel;
		}

		public static string TrimIndent(this string str, AttributeListSyntax node)
		{
			if (str.IsNullOrWhiteSpace() || node == null)
				return str;

			var triviaCount = node.GetAttributeIndentLevel();

			if (triviaCount == 0)
				return str;

			var sb = new System.Text.StringBuilder(string.Empty, capacity: str.Length);
			int counter = 0;

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];

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
	}
}

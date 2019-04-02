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
			Tooltip = GetAttributeTooltip();
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

		private string GetAttributeTooltip()
		{
			var cancellationToken = Tree.CodeMapViewModel.CancellationToken.GetValueOrDefault();
			var attributeListNode = Attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.Parent as AttributeListSyntax;

			if (attributeListNode == null)
				return $"[{Attribute.ToString()}]";

			string tooltip = attributeListNode.GetAttributeListStringWithRemovedIndent()
											  .RemoveCommonAcumaticaNamespacePrefixes();
			return tooltip ?? $"[{Attribute.ToString()}]";
		}
	}
}

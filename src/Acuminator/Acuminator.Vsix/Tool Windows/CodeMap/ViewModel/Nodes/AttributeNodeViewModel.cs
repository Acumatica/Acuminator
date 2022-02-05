using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class AttributeNodeViewModel : TreeNodeViewModel, IElementWithTooltip
	{
		private const string AttributeSuffix = nameof(System.Attribute);
		public AttributeData Attribute { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public override bool DisplayNodeWithoutChildren => true;

		public override Icon NodeIcon => Icon.None;

		private readonly Lazy<TooltipInfo> _tooltipLazy;

		public AttributeNodeViewModel(TreeNodeViewModel nodeVM, AttributeData attribute, bool isExpanded = false) :
								 base(nodeVM?.Tree, nodeVM, isExpanded)
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

			_tooltipLazy = new Lazy<TooltipInfo>(CalculateAttributeTooltip);
		}

		public async override Task NavigateToItemAsync()
		{
			if (Attribute.ApplicationSyntaxReference?.SyntaxTree == null)
				return;

			TextSpan span = Attribute.ApplicationSyntaxReference.Span;
			string filePath =  Attribute.ApplicationSyntaxReference.SyntaxTree.FilePath;
			Workspace workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (workspace?.CurrentSolution == null)
				return;

			await AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPositionAsync(workspace.CurrentSolution, filePath, span);
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);

		public TooltipInfo CalculateTooltip() => _tooltipLazy.Value;

		private TooltipInfo CalculateAttributeTooltip()
		{
			var cancellationToken = Tree.CodeMapViewModel.CancellationToken.GetValueOrDefault();
			var attributeListNode = Attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.Parent as AttributeListSyntax;
			string tooltip;

			if (attributeListNode == null || Tree.CodeMapViewModel.Workspace == null)
			{
				tooltip = $"[{Attribute.ToString()}]";
			}
			else
			{
				int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
				tooltip = attributeListNode.GetSyntaxNodeStringWithRemovedIndent(tabSize)
										   .RemoveCommonAcumaticaNamespacePrefixes();
				tooltip = tooltip.NullIfWhiteSpace() ?? $"[{Attribute.ToString()}]";
			}

			return new TooltipInfo(tooltip);
		}
	}
}

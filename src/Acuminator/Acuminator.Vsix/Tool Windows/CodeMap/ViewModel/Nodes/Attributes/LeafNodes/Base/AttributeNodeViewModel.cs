#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.ProjectSystem;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;	
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class AttributeNodeViewModel : TreeNodeViewModel, IElementWithTooltip
	{
		protected const string AttributeSuffix = nameof(System.Attribute);

		public AttributeInfoBase AttributeInfo { get; }

		public override string Name
		{
			get;
			protected set;
		}

		public override bool DisplayNodeWithoutChildren => true;

		public override Icon NodeIcon => Icon.Attribute;

		public override bool IconDependsOnCurrentTheme => true;

		private readonly Lazy<TooltipInfo> _tooltipLazy;

		public AttributePlacement Placement => AttributeInfo.Placement;

		protected AttributeNodeViewModel(TreeNodeViewModel nodeVM, AttributeInfoBase attributeInfo, bool isExpanded = false) :
									base(nodeVM?.Tree!, nodeVM, isExpanded)
		{
			AttributeInfo = attributeInfo.CheckIfNull();
			int lastDotIndex = AttributeInfo.AttributeType?.Name.LastIndexOf('.') ?? -1;
			string attributeName = lastDotIndex >= 0 && lastDotIndex < AttributeInfo.AttributeType!.Name.Length - 1
				? AttributeInfo.AttributeType.Name.Substring(lastDotIndex + 1)
				: AttributeInfo.Name;

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
			var syntaxReference = AttributeInfo.AttributeData.ApplicationSyntaxReference;

			if (syntaxReference?.SyntaxTree == null)
				return;

			TextSpan span = syntaxReference.Span;
			string filePath = syntaxReference.SyntaxTree.FilePath;
			Workspace workspace = await AcuminatorVSPackage.Instance.GetVSWorkspaceAsync();

			if (workspace?.CurrentSolution == null)
				return;

			await AcuminatorVSPackage.Instance.OpenCodeFileAndNavigateToPositionAsync(workspace.CurrentSolution, filePath, span);
		}

		public TooltipInfo CalculateTooltip() => _tooltipLazy.Value;

		private TooltipInfo CalculateAttributeTooltip()
		{
			var cancellationToken = Tree.CodeMapViewModel.CancellationToken.GetValueOrDefault();
			var attributeListNode = AttributeInfo.AttributeData.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.Parent as AttributeListSyntax;
			string tooltip;

			if (attributeListNode == null || Tree.CodeMapViewModel.Workspace == null)
			{
				tooltip = $"[{AttributeInfo.ToString()}]";
			}
			else
			{
				int tabSize = Tree.CodeMapViewModel.Workspace.GetWorkspaceIndentationSize();
				tooltip = attributeListNode.GetSyntaxNodeStringWithRemovedIndent(tabSize)
										   .RemoveCommonAcumaticaNamespacePrefixes();
				tooltip = tooltip.NullIfWhiteSpace() ?? $"[{AttributeInfo.ToString()}]";
			}

			return new TooltipInfo(tooltip);
		}
	}
}

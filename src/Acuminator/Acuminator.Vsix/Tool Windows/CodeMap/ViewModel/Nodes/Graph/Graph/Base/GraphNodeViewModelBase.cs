#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class GraphNodeViewModelBase : TreeNodeViewModel
	{
		public override Icon NodeIcon => IsGraph
			? Icon.Graph
			: Icon.GraphExtension;

		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public override string Name
		{
			get => GraphOrGraphExtInfo.Name;
			protected set { }
		}

		public abstract GraphOrGraphExtInfoBase GraphOrGraphExtInfo { get; }

		public bool IsGraph => GraphOrGraphExtInfo is GraphInfo;

		protected GraphNodeViewModelBase(TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
									base(tree, parent, isExpanded)
		{
		}

		protected TextViewModel CreateGraphTypeInfo()
		{
			Color color = Color.FromRgb(38, 155, 199);

			string graphType = IsGraph
				? VSIXResource.CodeMap_ExtraInfo_IsGraph
				: VSIXResource.CodeMap_ExtraInfo_IsGraphExtension;

			return new TextViewModel(this, graphType, darkThemeForeground: color, lightThemeForeground: color);
		}
	}
}
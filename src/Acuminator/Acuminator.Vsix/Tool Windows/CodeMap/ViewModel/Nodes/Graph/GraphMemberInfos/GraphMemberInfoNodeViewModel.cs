using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using System.Threading.Tasks;
using System.Threading;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberInfoNodeViewModel : TreeNodeViewModel, INodeWithSymbolItem
	{
		public GraphMemberNodeViewModel GraphMember { get; }

		public SymbolItem GraphMemberInfoData { get; }

		SymbolItem INodeWithSymbolItem.Symbol => GraphMemberInfoData;

		public ISymbol GraphMemberInfoSymbol => GraphMemberInfoData.SymbolBase;

		public GraphMemberInfoType GraphMemberInfoType { get; }

		public override string Name
		{
			get => GraphMemberInfoSymbol.Name;
			protected set { }
		}

		public override Icon NodeIcon
		{
			get
			{
				switch (GraphMemberInfoType)
				{
					case GraphMemberInfoType.ViewDelegate:
						return Icon.ViewDelegate;
					case GraphMemberInfoType.ActionHandler:
						return Icon.ActionHandler;
					default:
						return base.NodeIcon;
				}
			}
		}

		public override bool DisplayNodeWithoutChildren => true;

		public GraphMemberInfoNodeViewModel(GraphMemberNodeViewModel graphMemberVM, SymbolItem memberInfoData, 
											GraphMemberInfoType graphMemberInfoType, bool isExpanded = false) :
									  base(graphMemberVM?.Tree, isExpanded)
		{
			memberInfoData.ThrowOnNull(nameof(memberInfoData));

			GraphMemberInfoData = memberInfoData;
			GraphMember = graphMemberVM;
			GraphMemberInfoType = graphMemberInfoType;
		}

		public override Task NavigateToItemAsync() => GraphMemberInfoSymbol.NavigateToAsync();

		protected override IEnumerable<TreeNodeViewModel> CreateChildren(TreeBuilderBase treeBuilder, bool expandChildren, CancellationToken cancellation) =>
			treeBuilder.VisitNodeAndBuildChildren(this, expandChildren, cancellation);
	}
}

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.ToolWindows.CodeMap.Filter;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberInfoNodeViewModel : TreeNodeViewModel, INodeWithDeclarationOrder
	{
		public override TreeNodeFilterBehavior FilterBehavior => TreeNodeFilterBehavior.DisplayedIfNodeOrChildrenMeetFilter;

		public GraphMemberNodeViewModel GraphMember { get; }

		public SymbolItem GraphMemberInfoData { get; }

		public int DeclarationOrder => GraphMemberInfoData.DeclarationOrder;

		public ISymbol GraphMemberInfoSymbol => GraphMemberInfoData.SymbolBase;

		public GraphMemberInfoType GraphMemberInfoType { get; }

		public override Icon NodeIcon => GetIconType(GraphMemberInfoType);

		public override string Name
		{
			get => GraphMemberInfoSymbol.Name;
			protected set { }
		}

		public GraphMemberInfoNodeViewModel(GraphMemberNodeViewModel graphMemberVM, SymbolItem memberInfoData, 
											GraphMemberInfoType graphMemberInfoType, bool isExpanded) :
										base(graphMemberVM?.Tree!, graphMemberVM, isExpanded)
		{
			GraphMemberInfoData = memberInfoData.CheckIfNull();
			GraphMember = graphMemberVM!;
			GraphMemberInfoType = graphMemberInfoType;
		}

		public override Task NavigateToItemAsync() =>
			TryNavigateToItemWithVisualStudioWorkspace(GraphMemberInfoSymbol)
				? Task.CompletedTask
				: GraphMemberInfoSymbol.NavigateToAsync();

		private static Icon GetIconType(GraphMemberInfoType graphMemberInfoType) =>
			graphMemberInfoType switch
			{
				GraphMemberInfoType.ViewDelegate  => Icon.ViewDelegate,
				GraphMemberInfoType.ActionHandler => Icon.ActionHandler,
				_                                 => Icon.None,
			};

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

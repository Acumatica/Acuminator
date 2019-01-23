using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphMemberInfoNodeViewModel : TreeNodeViewModel
	{
		public GraphMemberNodeViewModel GraphMember { get; }

		public GraphNodeSymbolItem GraphMemberInfoData { get; }

		public ISymbol GraphMemberInfoSymbol => GraphMemberInfoData.SymbolBase;

		public GraphMemberInfoType GraphMemberInfoType { get; }

		public override string Name
		{
			get => GraphMemberInfoSymbol.Name;
			protected set { }
		}

		public GraphMemberInfoNodeViewModel(GraphMemberNodeViewModel graphMemberVM, GraphNodeSymbolItem memberInfoData, 
											GraphMemberInfoType graphMemberInfoType, bool isExpanded = false) :
									  base(graphMemberVM?.Tree, isExpanded)
		{
			memberInfoData.ThrowOnNull(nameof(memberInfoData));

			GraphMemberInfoData = memberInfoData;
			GraphMember = graphMemberVM;
			GraphMemberInfoType = graphMemberInfoType;
		}

		public override void NavigateToItem() => AcuminatorVSPackage.Instance.NavigateToSymbol(GraphMemberInfoSymbol);
	}
}

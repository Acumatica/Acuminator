#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacBaseTypesCategoryNodeViewModel : DacMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => false;

		public override Icon NodeIcon => Icon.Category;

		public DacInfo? BaseDacInfo { get; }

		public DacExtensionInfo? BaseDacExtensionInfo { get; }

		public DacBaseTypesCategoryNodeViewModel(DacNodeViewModel dacViewModel, TreeNodeViewModel parent, bool isExpanded) : 
											base(dacViewModel, parent, DacMemberCategory.BaseTypes, isExpanded)
		{
			if (dacViewModel.DacModel.DacType == DacType.Dac)
			{
				BaseDacInfo = DacViewModel.DacModelForCodeMap.DacInfo?.Base;
				BaseDacExtensionInfo = null;
			}
			else
			{
				BaseDacInfo = DacViewModel.DacModelForCodeMap.DacInfo;
				BaseDacExtensionInfo = DacViewModel.DacModelForCodeMap.DacExtensionInfo?.Base;
			}
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

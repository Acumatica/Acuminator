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

		public DacInfo? BaseDacInfo => DacViewModel.DacModelForCodeMap.BaseDacInfo;

		public DacExtensionInfo? BaseDacExtensionInfo => DacViewModel.DacModelForCodeMap.BaseDacExtensionInfo;

		public DacBaseTypesCategoryNodeViewModel(DacNodeViewModel dacViewModel, TreeNodeViewModel parent, bool isExpanded) : 
											base(dacViewModel, parent, DacMemberCategory.BaseTypes, isExpanded)
		{		
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

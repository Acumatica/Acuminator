#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class AllDacFieldsDacCategoryNodeViewModel : DacFieldCategoryNodeViewModel
	{
		public override Icon NodeIcon => Icon.AllDacFieldsCategory;

		protected override bool AllowNavigation => true;

		public AllDacFieldsDacCategoryNodeViewModel(DacNodeViewModel dacViewModel, TreeNodeViewModel parent, 
													ExpandCreatedNodesFunction isExpandedCalculator) : 
												base(dacViewModel, parent, DacMemberCategory.AllDacFields, isExpandedCalculator)
		{
		}

		public override IEnumerable<DacFieldInfo> GetCategoryDacFields() => DacModel.DeclaredDacFields.Where(dacField => !dacField.IsNonBqlProperty);

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

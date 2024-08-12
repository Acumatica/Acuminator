#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphAttributesGroupNodeViewModel : AttributesGroupNodeViewModel<GraphAttributeInfo>
	{
		public PXGraphEventSemanticModel GraphModel { get; }

		public override AttributePlacement Placement => AttributePlacement.Graph;

		protected override string AttributesGroupDescription => VSIXResource.CodeMap_Node_GraphAttributes;

		protected override bool AllowNavigation => true;

		public GraphAttributesGroupNodeViewModel(PXGraphEventSemanticModel graphSemanticModel, TreeNodeViewModel parent, bool isExpanded = false) :
												 base(parent, isExpanded)
		{
			GraphModel = graphSemanticModel.CheckIfNull();
		}

		public override IEnumerable<GraphAttributeInfo> AttributeInfos() => GraphModel.Attributes;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => 
			treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacAttributesGroupNodeViewModel : AttributesGroupNodeViewModel<DacAttributeInfo>
	{
		public DacSemanticModel DacModel { get; }

		public override AttributePlacement Placement => AttributePlacement.Dac;

		protected override string AttributesGroupDescription => VSIXResource.CodeMap_Node_DacAttributes;

		protected override bool AllowNavigation => true;

		public DacAttributesGroupNodeViewModel(DacSemanticModel dacSemanticModel, TreeNodeViewModel parent, bool isExpanded = false) :
											   base(parent, isExpanded)
		{	
			DacModel = dacSemanticModel.CheckIfNull();
		}

		public override IEnumerable<DacAttributeInfo> AttributeInfos() => DacModel.Attributes;

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => 
			treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

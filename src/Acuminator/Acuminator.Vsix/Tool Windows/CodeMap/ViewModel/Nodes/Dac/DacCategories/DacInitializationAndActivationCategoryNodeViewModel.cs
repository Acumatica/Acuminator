﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacInitializationAndActivationCategoryNodeViewModel : DacMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public override Icon NodeIcon => Icon.InitializationAndActivationDacCategory;

		public DacInitializationAndActivationCategoryNodeViewModel(DacNodeViewModel dacViewModel, bool isExpanded) : 
															  base(dacViewModel, DacMemberCategory.InitializationAndActivation, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryDacNodeSymbols() =>
			DacModel.IsActiveMethodInfo?.ToEnumerable() ?? Enumerable.Empty<SymbolItem>();

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}

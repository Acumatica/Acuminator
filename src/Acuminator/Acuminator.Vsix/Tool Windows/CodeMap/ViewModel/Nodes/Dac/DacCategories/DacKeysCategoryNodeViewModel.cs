using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacKeysCategoryNodeViewModel : DacMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public override Icon NodeIcon => Icon.DacKeysCategory;

		public DacKeysCategoryNodeViewModel(DacNodeViewModel dacViewModel, bool isExpanded) : 
									   base(dacViewModel, DacMemberCategory.Keys, isExpanded)
		{		
		}

		public override IEnumerable<SymbolItem> GetCategoryDacNodeSymbols() => DacModel.DeclaredDacProperties.Where(p => p.IsKey);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor, CancellationToken cancellationToken) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}

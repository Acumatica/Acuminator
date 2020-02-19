using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;



namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// A ViewModel for the view category CodeMap node.
	/// </summary>
	public class ViewCategoryNodeViewModel : GraphMemberCategoryNodeViewModel
	{
		protected override bool AllowNavigation => true;

		public ViewCategoryNodeViewModel(GraphNodeViewModel graphViewModel, bool isExpanded) : 
									base(graphViewModel, GraphMemberType.View, isExpanded)
		{			
		}

		public override IEnumerable<SymbolItem> GetCategoryGraphNodeSymbols() => GraphSemanticModel.Views;

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}

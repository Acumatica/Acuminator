using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.Utilities.Navigation;
using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacNodeViewModel : TreeNodeViewModel
	{
		public DacSemanticModel DacModel { get; }

		public override string Name
		{
			get => DacModel.Symbol.Name;
			protected set { }
		}

		public override Icon NodeIcon => Icon.Dac;

		public override bool DisplayNodeWithoutChildren => true;

		public DacNodeViewModel(DacSemanticModel dacModel, TreeViewModel tree, bool isExpanded) : base(tree, isExpanded)
		{
			dacModel.ThrowOnNull(nameof(dacModel));
			DacModel = dacModel;
		}

		public override Task NavigateToItemAsync() => DacModel.Symbol.NavigateToAsync();

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) =>
			treeVisitor.VisitNode(this, cancellationToken);
	}
}
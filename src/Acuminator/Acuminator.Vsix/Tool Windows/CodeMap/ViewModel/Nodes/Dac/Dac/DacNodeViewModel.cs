#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Vsix.ToolWindows.CodeMap.Dac;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DacNodeViewModel : DacNodeViewModelBase, IElementWithTooltip
	{
		public DacSemanticModelForCodeMap DacModelForCodeMap { get; }

		public DacSemanticModel DacModel => DacModelForCodeMap.DacModel;

		public override string Name
		{
			get => DacModelForCodeMap.Name;
			protected set { }
		}

		public override Icon NodeIcon => DacModelForCodeMap.DacType == DacType.Dac
			? Icon.Dac
			: Icon.DacExtension;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public DacNodeViewModel(DacSemanticModelForCodeMap dacModel, TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
						   base(tree, parent, isExpanded)
		{
			DacModelForCodeMap = dacModel.CheckIfNull();
			ExtraInfos		   = new ExtendedObservableCollection<ExtraInfoViewModel>(GetDacExtraInfos(DacModelForCodeMap));
		}

		public override Task NavigateToItemAsync() => DacModelForCodeMap.Symbol.NavigateToAsync();

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => 
			treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
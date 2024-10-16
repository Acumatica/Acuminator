﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;
using Acuminator.Vsix.ToolWindows.Common;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphNodeViewModel : GraphNodeViewModelBase, IElementWithTooltip
	{
		private int _currentNavigationIndex;

		public GraphSemanticModelForCodeMap CodeMapGraphModel { get; }

		public PXGraphEventSemanticModel GraphSemanticModel => CodeMapGraphModel.GraphModel; 

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public override GraphOrGraphExtInfoBase GraphOrGraphExtInfo => GraphSemanticModel.GraphOrGraphExtInfo;

		public GraphNodeViewModel(GraphSemanticModelForCodeMap codeMapGraphModel, TreeViewModel tree, TreeNodeViewModel? parent, bool isExpanded) : 
							 base(tree, parent, isExpanded)
		{
			CodeMapGraphModel = codeMapGraphModel;
			ExtraInfos = new ExtendedObservableCollection<ExtraInfoViewModel>(GetGraphExtraInfos());
		}

		private IEnumerable<ExtraInfoViewModel> GetGraphExtraInfos()
		{
			if (GraphSemanticModel.IsProcessing)
			{
				yield return new IconViewModel(this, Icon.Processing);
			}

			yield return CreateGraphTypeInfo();
		}

		public override Task NavigateToItemAsync()
		{
			if (TryNavigateToItemWithVisualStudioWorkspace(GraphSemanticModel.Symbol) || GraphSemanticModel.IsInMetadata)
				return Task.CompletedTask;

			var syntaxReferences = GraphSemanticModel.Symbol.DeclaringSyntaxReferences;

			switch (syntaxReferences.Length)
			{
				case 0:
					return Task.CompletedTask;
				case 1:
					return GraphSemanticModel.Symbol.NavigateToAsync();
				default:

					if (_currentNavigationIndex >= syntaxReferences.Length)
						return Task.CompletedTask;

					SyntaxReference reference = syntaxReferences[_currentNavigationIndex];
					_currentNavigationIndex = (_currentNavigationIndex + 1) % syntaxReferences.Length;

					return GraphSemanticModel.Symbol.NavigateToAsync(reference);
			}		
		}

		TooltipInfo? IElementWithTooltip.CalculateTooltip()
		{
			var graphAttributesGroupNode = AllChildren.OfType<GraphAttributesGroupNodeViewModel>().FirstOrDefault();
			return graphAttributesGroupNode is IElementWithTooltip elementWithTooltip
				? elementWithTooltip.CalculateTooltip()
				: null;
		}

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
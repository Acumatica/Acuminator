﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;
using Acuminator.Vsix.Utilities.Navigation;
using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class GraphNodeViewModel : TreeNodeViewModel
	{
		private int _currentNavigationIndex;

		public GraphSemanticModelForCodeMap CodeMapGraphModel { get; }

		public PXGraphEventSemanticModel GraphSemanticModel => CodeMapGraphModel.GraphModel; 

		public override string Name
		{
			get => GraphSemanticModel.Symbol.Name;
			protected set { }
		}

		public override Icon NodeIcon => GraphSemanticModel.Type switch
		{
			GraphType.PXGraph          => Icon.Graph,
			GraphType.PXGraphExtension => Icon.GraphExtension,
			_                          => Icon.None
		};

		public override bool DisplayNodeWithoutChildren => true;

		public override ExtendedObservableCollection<ExtraInfoViewModel> ExtraInfos { get; }

		public GraphNodeViewModel(GraphSemanticModelForCodeMap codeMapGraphModel, TreeViewModel tree, bool isExpanded) : 
							 base(tree, parent: null, isExpanded)
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

			if (GraphSemanticModel.Type == GraphType.None)
				yield break;

			Color color = Color.FromRgb(38, 155, 199);
			string graphType = GraphSemanticModel.Type == GraphType.PXGraph
				? VSIXResource.CodeMap_ExtraInfo_IsGraph
				: VSIXResource.CodeMap_ExtraInfo_IsGraphExtension;
			yield return new TextViewModel(this, graphType, darkThemeForeground: color, lightThemeForeground: color);
		}

		public override Task NavigateToItemAsync()
		{
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

		public override TResult AcceptVisitor<TInput, TResult>(CodeMapTreeVisitor<TInput, TResult> treeVisitor, TInput input) => treeVisitor.VisitNode(this, input);

		public override TResult AcceptVisitor<TResult>(CodeMapTreeVisitor<TResult> treeVisitor) => treeVisitor.VisitNode(this);

		public override void AcceptVisitor(CodeMapTreeVisitor treeVisitor) => treeVisitor.VisitNode(this);
	}
}
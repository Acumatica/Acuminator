using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected override IEnumerable<TreeNodeViewModel> CreateRoots(TreeViewModel tree, bool expandRoots, CancellationToken cancellation)
		{
			if (tree.CodeMapViewModel.DocumentModel == null)
				yield break;

			foreach (GraphSemanticModelForCodeMap graph in tree.CodeMapViewModel.DocumentModel.GraphModels)
			{
				cancellation.ThrowIfCancellationRequested();
				yield return CreateGraphNode(graph, tree, expandRoots);
			}
		}

		protected override IEnumerable<TreeNodeViewModel> GetChildrenNodes(TreeNodeViewModel parentNode, bool expandChildren,
																		   CancellationToken cancellation)
		{
			switch (parentNode)
			{
				case GraphNodeViewModel graphNode:
					return CreateGraphCategories(graphNode, expandChildren, cancellation);

				case GraphEventCategoryNodeViewModel graphEventCategory:
					return CreateEventsCategoryChildren(graphEventCategory, expandChildren, cancellation);

				case GraphMemberCategoryNodeViewModel graphMemberCategory:
					return CreateGraphCategoryChildren(graphMemberCategory, expandChildren, cancellation);

				default:
					return null;
			}
		}

		protected virtual GraphNodeViewModel CreateGraphNode(GraphSemanticModelForCodeMap graph, TreeViewModel tree, bool expand) =>
			new GraphNodeViewModel(graph, tree, expand);

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategories(GraphNodeViewModel graph, bool isExpanded, 
																			   CancellationToken cancellation)
		{
			foreach (GraphMemberType graphMemberType in GetGraphMemberTypesInOrder())
			{
				cancellation.ThrowIfCancellationRequested();
				GraphMemberCategoryNodeViewModel graphMemberCategory = CreateCategory(graph, graphMemberType, isExpanded);

				if (graphMemberCategory != null)
					yield return graphMemberCategory;
			}
		}

		protected virtual IEnumerable<GraphMemberType> GetGraphMemberTypesInOrder()
		{
			yield return GraphMemberType.View;
			yield return GraphMemberType.Action;
			yield return GraphMemberType.PXOverride;
			yield return GraphMemberType.CacheAttached;
			yield return GraphMemberType.RowEvent;
			yield return GraphMemberType.FieldEvent;
			yield return GraphMemberType.NestedDAC;
			yield return GraphMemberType.NestedGraph;
		}

		protected virtual GraphMemberCategoryNodeViewModel CreateCategory(GraphNodeViewModel graph, GraphMemberType graphMemberType, bool isExpanded)
		{
			switch (graphMemberType)
			{
				case GraphMemberType.View:
					return new ViewCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.Action:
					return new ActionCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.CacheAttached:
					return new CacheAttachedCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.RowEvent:
					return new RowEventCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.FieldEvent:
					return new FieldEventCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.PXOverride:
					return new PXOverridesCategoryNodeViewModel(graph, isExpanded);
				case GraphMemberType.NestedDAC:
				case GraphMemberType.NestedGraph:
				default:
					return null;
			}
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategoryChildren(GraphMemberCategoryNodeViewModel graphMemberCategory, bool isExpanded,
																					 CancellationToken cancellation)
		{

		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateEventsCategoryChildren(GraphEventCategoryNodeViewModel graphEventCategory, bool isExpanded,
																					  CancellationToken cancellation)
		{
			var graphSemanticModel = graphEventCategory.GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = graphEventCategory.GetCategoryGraphNodeSymbols()
													   ?.OfType<GraphEventInfoBase>()
														.Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEvents.IsNullOrEmpty())
				return Enumerable.Empty<TreeNodeViewModel>();

			var graphMemberViewModels = from eventInfo in graphCategoryEvents
										where eventInfo.Symbol.ContainingType == graphSemanticModel.Symbol ||
											  eventInfo.Symbol.ContainingType.OriginalDefinition == graphSemanticModel.Symbol.OriginalDefinition
										group eventInfo by eventInfo.DacName into graphEventsForDAC
										select DacEventsGroupingNodeViewModel.Create(this, graphEventsForDAC.Key, graphEventsForDAC) into dacNodeVM
										where dacNodeVM != null
										orderby dacNodeVM.DacName ascending
										select dacNodeVM;

		}

		
	}
}

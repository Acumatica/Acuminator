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
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected delegate DacGroupingNodeBaseViewModel DacVmConstructor<TEventInfo>(GraphEventCategoryNodeViewModel graphEventCategory,
																					 string dacName, IEnumerable<TEventInfo> eventInfos)
		where TEventInfo : GraphEventInfoBase<TEventInfo>;

		protected delegate DacFieldGroupingNodeBaseViewModel DacFieldVmConstructor(DacGroupingNodeBaseViewModel dacNodeVm, string dacName,
																				   IEnumerable<GraphFieldEventInfo> fieldEvents);
		protected virtual GraphNodeViewModel CreateGraphNode(GraphSemanticModelForCodeMap graph, TreeViewModel tree) =>
			new GraphNodeViewModel(graph, tree, ExpandCreatedNodes);

		public override IEnumerable<TreeNodeViewModel> VisitNode(GraphNodeViewModel graph)
		{
			foreach (GraphMemberType graphMemberType in GetGraphMemberTypesInOrder())
			{
				Cancellation.ThrowIfCancellationRequested();
				GraphMemberCategoryNodeViewModel graphMemberCategory = CreateCategory(graph, graphMemberType);

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

		protected virtual GraphMemberCategoryNodeViewModel CreateCategory(GraphNodeViewModel graph, GraphMemberType graphMemberType)
		{
			switch (graphMemberType)
			{
				case GraphMemberType.View:
					return new ViewCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.Action:
					return new ActionCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.CacheAttached:
					return new CacheAttachedCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.RowEvent:
					return new RowEventCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.FieldEvent:
					return new FieldEventCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.PXOverride:
					return new PXOverridesCategoryNodeViewModel(graph, ExpandCreatedNodes);
				case GraphMemberType.NestedDAC:
				case GraphMemberType.NestedGraph:
				default:
					return null;
			}
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(ActionCategoryNodeViewModel actionCategory)
		{
			return CreateGraphCategoryChildren<ActionInfo>(actionCategory, 
						constructor: actionInfo => new ActionNodeViewModel(actionCategory, actionInfo, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(ViewCategoryNodeViewModel viewCategory)
		{
			return CreateGraphCategoryChildren<DataViewInfo>(viewCategory, 
						constructor: viewInfo => new ViewNodeViewModel(viewCategory, viewInfo, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory)
		{
			return CreateGraphCategoryChildren<PXOverrideInfoForCodeMap>(pxOverridesCategory,
						constructor: pxOverrideInfo => new PXOverrideNodeViewModel(pxOverridesCategory, pxOverrideInfo, ExpandCreatedNodes));
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategoryChildren<TSymbolInfo>(GraphMemberCategoryNodeViewModel graphMemberCategory,
																								  Func<TSymbolInfo, TreeNodeViewModel> constructor)
		where TSymbolInfo : SymbolItem
		{
			IEnumerable<SymbolItem> categoryTreeNodes = graphMemberCategory.CheckIfNull(nameof(graphMemberCategory))
																		   .GetCategoryGraphNodeSymbols();
			if (categoryTreeNodes.IsNullOrEmpty())
				return DefaultValue;

			Cancellation.ThrowIfCancellationRequested();
			var graphSemanticModel = graphMemberCategory.GraphSemanticModel;
			var graphMemberViewModels = from graphMemberInfo in categoryTreeNodes.OfType<TSymbolInfo>()
										where graphMemberInfo.SymbolBase.ContainingType == graphSemanticModel.Symbol ||
											  graphMemberInfo.SymbolBase.ContainingType.OriginalDefinition == graphSemanticModel.Symbol.OriginalDefinition
										select constructor(graphMemberInfo);

			var sorter = graphMemberCategory.Tree.CodeMapViewModel.TreeSorter;
			var sortedGraphMemberViewModels = sorter.SortNodes(graphMemberViewModels, SortType.Declaration, SortDirection.Ascending);
			return graphMemberViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(RowEventCategoryNodeViewModel rowEventCategory)
		{
			return CreateEventsCategoryChildren<GraphRowEventInfo>(rowEventCategory,
							(eventCategory, dacName, rowEvents) => new DacGroupingNodeForRowEventViewModel(eventCategory, dacName, rowEvents, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory)
		{
			return CreateEventsCategoryChildren<GraphFieldEventInfo>(cacheAttachedCategory,
							(eventCategory, dacName, fieldEvents) => new DacGroupingNodeForCacheAttachedEventViewModel(eventCategory, dacName, fieldEvents, ExpandCreatedNodes));
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(FieldEventCategoryNodeViewModel fieldEventCategory)
		{
			return CreateEventsCategoryChildren<GraphFieldEventInfo>(fieldEventCategory,
							(eventCategory, dacName, fieldEvents) => new DacGroupingNodeForFieldEventViewModel(eventCategory, dacName, fieldEvents, ExpandCreatedNodes));
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateEventsCategoryChildren<TGraphEventInfo>(GraphEventCategoryNodeViewModel graphEventCategory,
																									   DacVmConstructor<TGraphEventInfo> constructor)
		where TGraphEventInfo : GraphEventInfoBase<TGraphEventInfo>
		{
			graphEventCategory.ThrowOnNull(nameof(graphEventCategory));

			var graphSemanticModel = graphEventCategory.GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = graphEventCategory.GetCategoryGraphNodeSymbols()
													   ?.OfType<TGraphEventInfo>()
														.Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEvents.IsNullOrEmpty())
				return Enumerable.Empty<TreeNodeViewModel>();

			Cancellation.ThrowIfCancellationRequested();
			var dacGroupingNodesViewModels = from eventInfo in graphCategoryEvents
											 where eventInfo.Symbol.ContainingType == graphSemanticModel.Symbol ||
												   eventInfo.Symbol.ContainingType.OriginalDefinition == graphSemanticModel.Symbol.OriginalDefinition
											 group eventInfo by eventInfo.DacName into graphEventsForDAC
											 select constructor(graphEventCategory, graphEventsForDAC.Key, graphEventsForDAC) into dacNodeVM
											 where dacNodeVM != null
											 select dacNodeVM;

			var sorter = graphEventCategory.Tree.CodeMapViewModel.TreeSorter;
			var sortedDacGroupingNodesViewModels = sorter.SortNodes(dacGroupingNodesViewModels, SortType.Alphabet, SortDirection.Ascending);
			return sortedDacGroupingNodesViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode)
		{
			return dacGroupingNode.RowEvents.Select(rowEventInfo => new RowEventNodeViewModel(dacGroupingNode, rowEventInfo, ExpandCreatedNodes))
											.Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty())
											.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode)
		{
			return dacGroupingNode?.AllFieldEvents.Select(fieldEvent => new CacheAttachedNodeViewModel(dacGroupingNode, fieldEvent, ExpandCreatedNodes))
												  .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty())
												  .OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode)
		{
			return CreateDacChildrenForFieldEvents(dacGroupingNode,
						constructor: (dacVm, dacFieldName, dacFieldEvents) => new DacFieldGroupingNodeForFieldEventViewModel(dacVm, dacFieldName, dacFieldEvents, ExpandCreatedNodes));
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacChildrenForFieldEvents(DacGroupingNodeForFieldEventViewModel dacEventsGroupingNode,
																						 DacFieldVmConstructor constructor)
		{
			return from eventInfo in dacEventsGroupingNode.AllFieldEvents
				   group eventInfo by eventInfo.DacFieldName
						into dacFieldEvents
				   select constructor(dacEventsGroupingNode, dacFieldEvents.Key, dacFieldEvents)
						into dacFieldNodeVM
				   where !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   orderby dacFieldNodeVM.DacFieldName ascending
				   select dacFieldNodeVM;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode)
		{
			return dacFieldGroupingNode?.FieldEvents.Select(fieldEvent => new FieldEventNodeViewModel(dacFieldGroupingNode, fieldEvent, ExpandCreatedNodes))
													.Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty())
													.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(ViewNodeViewModel viewNode)
		{
			var hasViewDelegate = viewNode.MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(viewNode.MemberSymbol.Name,
																											  out DataViewDelegateInfo viewDelegate);
			return hasViewDelegate
				? new GraphMemberInfoNodeViewModel(viewNode, viewDelegate, GraphMemberInfoType.ViewDelegate).ToEnumerable()
				: DefaultValue;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(ActionNodeViewModel actionNode)
		{
			var hasActionHandler =
				actionNode.MemberCategory.GraphSemanticModel.ActionHandlersByNames.TryGetValue(actionNode.MemberSymbol.Name,
																							   out ActionHandlerInfo actionHandler);
			return hasActionHandler
				? new GraphMemberInfoNodeViewModel(actionNode, actionHandler, GraphMemberInfoType.ActionHandler).ToEnumerable()
				: DefaultValue;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedNodeViewModel cacheAttachedNode)
		{
			return cacheAttachedNode?.MemberSymbol.GetAttributes()
												  .Select(a => new AttributeNodeViewModel(cacheAttachedNode, a));
		}
	}
}
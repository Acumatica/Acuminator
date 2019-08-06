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
		protected delegate DacGroupingNodeBaseViewModel DacConstructor<TEventInfo>(GraphEventCategoryNodeViewModel graphEventCategory, 
																				   string dacName, IEnumerable<TEventInfo> eventInfos,
																				   bool expandChildren)
		where TEventInfo : GraphEventInfoBase<TEventInfo>;


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

		protected virtual GraphNodeViewModel CreateGraphNode(GraphSemanticModelForCodeMap graph, TreeViewModel tree, bool expand) =>
			new GraphNodeViewModel(graph, tree, expand);

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(GraphNodeViewModel graph, bool expandChildren, CancellationToken cancellation)
		{
			foreach (GraphMemberType graphMemberType in GetGraphMemberTypesInOrder())
			{
				cancellation.ThrowIfCancellationRequested();
				GraphMemberCategoryNodeViewModel graphMemberCategory = CreateCategory(graph, graphMemberType, expandChildren);

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


		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ActionCategoryNodeViewModel actionCategory, bool expandChildren, 
																				 CancellationToken cancellation)
		{
			return CreateGraphCategoryChildren<ActionInfo>(actionCategory, expandChildren, constructor: actionInfo => new ActionNodeViewModel(actionCategory, actionInfo), 
														   cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ViewCategoryNodeViewModel viewCategory, bool expandChildren, CancellationToken cancellation)
		{
			return CreateGraphCategoryChildren<DataViewInfo>(viewCategory, expandChildren, constructor: viewInfo => new ViewNodeViewModel(viewCategory, viewInfo),
														     cancellation);
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateGraphCategoryChildren<TSymbolInfo>(GraphMemberCategoryNodeViewModel graphMemberCategory,
																								  bool isExpanded, Func<TSymbolInfo, TreeNodeViewModel> constructor,
																								  CancellationToken cancellation)
		where TSymbolInfo : SymbolItem
		{
			constructor.ThrowOnNull(nameof(constructor));
			IEnumerable<SymbolItem> categoryTreeNodes = graphMemberCategory.CheckIfNull(nameof(graphMemberCategory))
																		   .GetCategoryGraphNodeSymbols();
			if (categoryTreeNodes.IsNullOrEmpty())
				return Enumerable.Empty<TreeNodeViewModel>();

			cancellation.ThrowIfCancellationRequested();
			var graphSemanticModel = graphMemberCategory.GraphSemanticModel;
			var graphMemberViewModels = from graphMemberInfo in categoryTreeNodes.OfType<TSymbolInfo>()
										where graphMemberInfo.SymbolBase.ContainingType == graphSemanticModel.Symbol ||
											  graphMemberInfo.SymbolBase.ContainingType.OriginalDefinition == graphSemanticModel.Symbol.OriginalDefinition
										orderby graphMemberInfo.SymbolBase.Name
										select constructor(graphMemberInfo);
			return graphMemberViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(RowEventCategoryNodeViewModel rowEventCategory, bool expandChildren,
																				 CancellationToken cancellation)
		{
			return CreateEventsCategoryChildren<GraphRowEventInfo>(rowEventCategory, 
							(eventCategory, dacName, rowEvents, isExpanded) => new DacGroupingNodeForRowEventViewModel(eventCategory, dacName, rowEvents, isExpanded),
							expandChildren, cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(CacheAttachedCategoryNodeViewModel cacheAttachedCategory,
																				 bool expandChildren, CancellationToken cancellation)
		{
			return CreateEventsCategoryChildren<GraphFieldEventInfo>(cacheAttachedCategory,
							(eventCategory, dacName, fieldEvents, isExpanded) => new DacGroupingNodeForCacheAttachedEventViewModel(eventCategory, dacName, fieldEvents, isExpanded),
							expandChildren, cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(FieldEventCategoryNodeViewModel fieldEventCategory,
																				 bool expandChildren, CancellationToken cancellation)
		{
			return CreateEventsCategoryChildren<GraphFieldEventInfo>(fieldEventCategory,
							(eventCategory, dacName, fieldEvents, isExpanded) => new DacGroupingNodeForFieldEventViewModel(eventCategory, dacName, fieldEvents, isExpanded),
							expandChildren, cancellation);
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateEventsCategoryChildren<TGraphEventInfo>(GraphEventCategoryNodeViewModel graphEventCategory,
																									   DacConstructor<TGraphEventInfo> constructor,
																									   bool expandChildren, CancellationToken cancellation)
		where TGraphEventInfo : GraphEventInfoBase<TGraphEventInfo>
		{
			graphEventCategory.ThrowOnNull(nameof(graphEventCategory));

			var graphSemanticModel = graphEventCategory.GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = graphEventCategory.GetCategoryGraphNodeSymbols()
													   ?.OfType<TGraphEventInfo>()
														.Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEvents.IsNullOrEmpty())
				return Enumerable.Empty<TreeNodeViewModel>();

			var dacGroupingNodesViewModels = from eventInfo in graphCategoryEvents
											 where eventInfo.Symbol.ContainingType == graphSemanticModel.Symbol ||
												   eventInfo.Symbol.ContainingType.OriginalDefinition == graphSemanticModel.Symbol.OriginalDefinition
											 group eventInfo by eventInfo.DacName into graphEventsForDAC
											 select constructor(graphEventCategory, graphEventsForDAC.Key, graphEventsForDAC, expandChildren) into dacNodeVM
											 where dacNodeVM != null
											 orderby dacNodeVM.DacName ascending
											 select dacNodeVM;

			return dacGroupingNodesViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacGroupingNodeForCacheAttachedEventViewModel dacGroupingNode,
																				 bool expandChildren, CancellationToken cancellation)
		{
			var cacheAttachedEvents = CreateRowEvents(dacGroupingNode, 
													  rowEventInfo => new CacheAttachedNodeViewModel(dacGroupingNode, rowEventInfo, expandChildren));
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacFieldEventChildren(DacGroupingNodeBaseViewModel dacEventsGroupingNode, bool expandChildren,
																					 CancellationToken cancellation)
		{
			return from eventInfo in dacEventsGroupingNode.GraphEventsForDAC.OfType<GraphFieldEventInfo>()
				   group eventInfo by eventInfo.DacFieldName
						into dacFieldEvents
				   select DacFieldGroupingNodeViewModel.Create(dacEventsGroupingNode, dacFieldEvents.Key, dacFieldEvents)
						into dacFieldNodeVM
				   where dacFieldNodeVM != null && !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   orderby dacFieldNodeVM.DacFieldName ascending
				   select dacFieldNodeVM;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacGroupingNodeForRowEventViewModel dacGroupingNode, bool expandChildren,
																				 CancellationToken cancellation)
		{
			return dacGroupingNode.RowEvents.Select(rowEventInfo => new RowEventNodeViewModel(dacGroupingNode, rowEventInfo, expandChildren))
											.Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty())
											.OrderBy(graphMemberVM => graphMemberVM.Name);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ViewNodeViewModel viewNode, bool expandChildren, 
																				 CancellationToken cancellation)
		{
			var hasViewDelegate = viewNode.MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(viewNode.MemberSymbol.Name,
																											  out DataViewDelegateInfo viewDelegate);
			return hasViewDelegate
				? new GraphMemberInfoNodeViewModel(viewNode, viewDelegate, GraphMemberInfoType.ViewDelegate).ToEnumerable()
				: base.VisitNodeAndBuildChildren(viewNode, expandChildren, cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ActionNodeViewModel actionNode, bool expandChildren,
																				 CancellationToken cancellation)
		{
			var hasActionHandler = actionNode.MemberCategory.GraphSemanticModel.ActionHandlersByNames.TryGetValue(actionNode.MemberSymbol.Name,
																												  out ActionHandlerInfo actionHandler);
			return hasActionHandler
				? new GraphMemberInfoNodeViewModel(actionNode, actionHandler, GraphMemberInfoType.ActionHandler).ToEnumerable()
				: base.VisitNodeAndBuildChildren(actionNode, expandChildren, cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(CacheAttachedNodeViewModel cacheAttachedNode, bool expandChildren,
																				 CancellationToken cancellation)
		{
			return cacheAttachedNode?.MemberSymbol.GetAttributes()
												  .Select(a => new AttributeNodeViewModel(cacheAttachedNode, a));			
		}
	}
}

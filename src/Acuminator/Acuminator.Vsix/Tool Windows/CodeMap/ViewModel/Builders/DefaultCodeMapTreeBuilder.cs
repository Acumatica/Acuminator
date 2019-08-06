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
										select DacGroupingNodeBaseViewModel.Create(this, graphEventsForDAC.Key, graphEventsForDAC) into dacNodeVM
										where dacNodeVM != null
										orderby dacNodeVM.DacName ascending
										select dacNodeVM;

		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(DacGroupingNodeBaseViewModel dacEventsGroupingNode, bool expandChildren,
																				 CancellationToken cancellation)
		{
			switch (dacEventsGroupingNode.GraphEventsCategoryVM)
			{
				case RowEventCategoryNodeViewModel _:
				case CacheAttachedCategoryNodeViewModel _:
					return CreateDacRowEventChildren(dacEventsGroupingNode, expandChildren, cancellation);
				case FieldEventCategoryNodeViewModel _:
					return CreateDacFieldEventChildren(dacEventsGroupingNode, expandChildren, cancellation);
				default:
					return base.VisitNodeAndBuildChildren(dacEventsGroupingNode, expandChildren, cancellation);
			}

			return eventNodeParent is DacGroupingNodeBaseViewModel dacGroupVM && eventInfo is GraphFieldEventInfo fieldEventInfo
			? new CacheAttachedNodeViewModel(dacGroupVM, fieldEventInfo, isExpanded)
			: base.CreateNewEventVM(eventNodeParent, eventInfo, isExpanded);
		}

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacRowEventChildren(DacGroupingNodeBaseViewModel dacEventsGroupingNode,
																				   bool expandChildren, Func<GraphRowEventInfo, TreeNodeViewModel> constructor,
																				   CancellationToken cancellation)
		{ 
			var rowEvents = from rowEventInfo in dacEventsGroupingNode.GraphEventsForDAC.OfType<GraphRowEventInfo>()
							where 
								 .Select(eventInfo => constructor(eventInfo))
								 .Where(graphMemberVM => graphMemberVM != null && !graphMemberVM.Name.IsNullOrEmpty())
								 .OrderBy(graphMemberVM => graphMemberVM.Name);
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

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ViewNodeViewModel viewNode, bool expandChildren, 
																				 CancellationToken cancellation)
		{
			var hasViewDelegate = viewNode.MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(viewNode.MemberSymbol.Name,
																											  out DataViewDelegateInfo viewDelegate);
			return hasViewDelegate
				? new GraphMemberInfoNodeViewModel(viewNode, viewDelegate, GraphMemberInfoType.ViewDelegate).ToEnumerable()
				: base.VisitNodeAndBuildChildren(viewNode, expandChildren, cancellation);
		}

		public override IEnumerable<TreeNodeViewModel> VisitNodeAndBuildChildren(ActionNodeViewModel actionNode, bool expandChildren, CancellationToken cancellation)
		{
			var hasActionHandler = actionNode.MemberCategory.GraphSemanticModel.ActionHandlersByNames.TryGetValue(actionNode.MemberSymbol.Name,
																												  out ActionHandlerInfo actionHandler);
			return hasActionHandler
				? new GraphMemberInfoNodeViewModel(actionNode, actionHandler, GraphMemberInfoType.ActionHandler).ToEnumerable()
				: base.VisitNodeAndBuildChildren(actionNode, expandChildren, cancellation);
		}
	}
}

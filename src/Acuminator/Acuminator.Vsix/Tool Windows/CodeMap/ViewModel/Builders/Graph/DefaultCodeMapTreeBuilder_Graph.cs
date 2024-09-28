#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.SharedInfo;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

using Microsoft.CodeAnalysis;

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

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphNodeViewModel graph)
		{
			var graphAttributesGroup = GetGraphAttributesGroupNode(graph);

			if (graphAttributesGroup != null)
				yield return graphAttributesGroup;

			foreach (GraphMemberCategory graphMemberType in GetGraphMemberTypesInOrder())
			{
				Cancellation.ThrowIfCancellationRequested();
				GraphMemberCategoryNodeViewModel? graphMemberCategory = CreateCategory(graph, graphMemberType);

				if (graphMemberCategory != null)
					yield return graphMemberCategory;
			}
		}

		protected virtual GraphAttributesGroupNodeViewModel GetGraphAttributesGroupNode(GraphNodeViewModel graph) =>
			new GraphAttributesGroupNodeViewModel(graph.GraphSemanticModel, graph, ExpandCreatedNodes);

		protected virtual IEnumerable<GraphMemberCategory> GetGraphMemberTypesInOrder()
		{
			yield return GraphMemberCategory.InitializationAndActivation;
			yield return GraphMemberCategory.View;
			yield return GraphMemberCategory.Action;
			yield return GraphMemberCategory.PXOverride;
			yield return GraphMemberCategory.BaseMemberOverride;
			yield return GraphMemberCategory.CacheAttached;
			yield return GraphMemberCategory.RowEvent;
			yield return GraphMemberCategory.FieldEvent;
			yield return GraphMemberCategory.NestedDAC;
			yield return GraphMemberCategory.NestedGraph;
		}

		protected virtual GraphMemberCategoryNodeViewModel? CreateCategory(GraphNodeViewModel graph, GraphMemberCategory graphMemberType) =>
			graphMemberType switch
			{
				GraphMemberCategory.InitializationAndActivation => new GraphInitializationAndActivationCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.View 						=> new ViewCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.Action 						=> new ActionCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.CacheAttached 				=> new CacheAttachedCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.RowEvent 					=> new RowEventCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.FieldEvent 					=> new FieldEventCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.PXOverride 					=> new PXOverridesCategoryNodeViewModel(graph, ExpandCreatedNodes),
				GraphMemberCategory.BaseMemberOverride 			=> new GraphBaseMemberOverridesCategoryNodeViewModel(graph, ExpandCreatedNodes),
				_ 											=> null,
			};

		public override IEnumerable<TreeNodeViewModel> VisitNode(GraphAttributesGroupNodeViewModel attributeGroupNode) =>
			attributeGroupNode.AttributeInfos()
							  .Select(attrInfo => new GraphAttributeNodeViewModel(attributeGroupNode, attrInfo, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategory)
		{
			return CreateGraphCategoryChildren<SymbolItem>(graphInitializationAndActivationCategory, InitializationAndActivationGraphMemberConstructor);

			//----------------------------------Local Function-----------------------------------------------------
			TreeNodeViewModel? InitializationAndActivationGraphMemberConstructor(SymbolItem symbolInfo) => symbolInfo switch
			{
				IsActiveMethodInfo isActiveMethodInfo 				  => new IsActiveGraphMethodNodeViewModel(graphInitializationAndActivationCategory,
																											  isActiveMethodInfo, ExpandCreatedNodes),
				InitializeMethodInfo initializeMethodInfo 			  => new GraphInitializeMethodNodeViewModel(graphInitializationAndActivationCategory,
																												initializeMethodInfo, ExpandCreatedNodes),
				StaticConstructorInfo staticConstructorInfo 		  => new GraphStaticConstructorNodeViewModel(graphInitializationAndActivationCategory,
																												 staticConstructorInfo, ExpandCreatedNodes),
				InstanceConstructorInfo instanceConstructorInfo 	  => new GraphInstanceConstructorNodeViewModel(graphInitializationAndActivationCategory,
																												   instanceConstructorInfo, ExpandCreatedNodes),
				ConfigureMethodInfo configureMethodInfo 			  => new GraphConfigureMethodNodeViewModel(graphInitializationAndActivationCategory, 
																											   configureMethodInfo, ExpandCreatedNodes),
				IsActiveForGraphMethodInfo isActiveForGraphMethodInfo => new IsActiveForGraphMethodNodeViewModel(graphInitializationAndActivationCategory,
																												 isActiveForGraphMethodInfo, ExpandCreatedNodes),
				_													  => null
			};
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategory) =>
			CreateGraphCategoryChildren<BaseMemberOverrideInfo>(graphBaseMemberOverridesCategory,
						constructor: baseMemberOverrideInfo => new GraphBaseMembeOverrideNodeViewModel(graphBaseMemberOverridesCategory,
																										baseMemberOverrideInfo, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ActionCategoryNodeViewModel actionCategory) =>
			CreateGraphCategoryChildren<ActionInfo>(actionCategory,
						constructor: actionInfo => new ActionNodeViewModel(actionCategory, actionInfo, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ViewCategoryNodeViewModel viewCategory) =>
			 CreateGraphCategoryChildren<DataViewInfo>(viewCategory,
						constructor: viewInfo => new ViewNodeViewModel(viewCategory, viewInfo, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory) =>
			CreateGraphCategoryChildren<PXOverrideInfo>(pxOverridesCategory,
						constructor: pxOverrideInfo => new PXOverrideNodeViewModel(pxOverridesCategory, pxOverrideInfo, ExpandCreatedNodes));

		protected virtual IEnumerable<TreeNodeViewModel>? CreateGraphCategoryChildren<TSymbolInfo>(GraphMemberCategoryNodeViewModel graphMemberCategory,
																								   Func<TSymbolInfo, TreeNodeViewModel?> constructor)
		where TSymbolInfo : SymbolItem
		{
			IEnumerable<SymbolItem> categoryTreeNodes = graphMemberCategory.CheckIfNull()
																		   .GetCategoryGraphNodeSymbols();
			if (categoryTreeNodes.IsNullOrEmpty())
				return DefaultValue;

			Cancellation.ThrowIfCancellationRequested();
			var graphSemanticModel = graphMemberCategory.GraphSemanticModel;
			var graphMemberViewModels = from graphMemberInfo in categoryTreeNodes.OfType<TSymbolInfo>()
										where graphMemberInfo.SymbolBase.IsDeclaredInType(graphSemanticModel.Symbol)
										select constructor(graphMemberInfo);

			return graphMemberViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(RowEventCategoryNodeViewModel rowEventCategory) =>
			CreateEventsCategoryChildren<GraphRowEventInfo>(rowEventCategory,
					(eventCategory, dacName, rowEvents) => new DacGroupingNodeForRowEventViewModel(eventCategory, dacName, rowEvents, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory) =>
			CreateEventsCategoryChildren<GraphFieldEventInfo>(cacheAttachedCategory,
					(eventCategory, dacName, fieldEvents) => new DacGroupingNodeForCacheAttachedEventViewModel(eventCategory, dacName, fieldEvents, ExpandCreatedNodes));

		public override IEnumerable<TreeNodeViewModel> VisitNode(FieldEventCategoryNodeViewModel fieldEventCategory) =>
			CreateEventsCategoryChildren<GraphFieldEventInfo>(fieldEventCategory,
					(eventCategory, dacName, fieldEvents) => new DacGroupingNodeForFieldEventViewModel(eventCategory, dacName, fieldEvents, ExpandCreatedNodes));

		protected virtual IEnumerable<TreeNodeViewModel> CreateEventsCategoryChildren<TGraphEventInfo>(GraphEventCategoryNodeViewModel graphEventCategory,
																									   DacVmConstructor<TGraphEventInfo> constructor)
		where TGraphEventInfo : GraphEventInfoBase<TGraphEventInfo>
		{
			graphEventCategory.ThrowOnNull();

			var graphSemanticModel = graphEventCategory.GraphViewModel.GraphSemanticModel;
			var graphCategoryEvents = graphEventCategory.GetCategoryGraphNodeSymbols()
													   ?.OfType<TGraphEventInfo>()
														.Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEvents.IsNullOrEmpty())
				return Enumerable.Empty<TreeNodeViewModel>();

			Cancellation.ThrowIfCancellationRequested();
			var dacGroupingNodesViewModels = from eventInfo in graphCategoryEvents
											 where eventInfo.Symbol.IsDeclaredInType(graphSemanticModel.Symbol)
											 group eventInfo by eventInfo.DacName into graphEventsForDAC
											 select constructor(graphEventCategory, graphEventsForDAC.Key, graphEventsForDAC) into dacNodeVM
											 where dacNodeVM != null
											 select dacNodeVM;

			return dacGroupingNodesViewModels;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacGroupingNodeForRowEventViewModel dacGroupingNode) =>
			dacGroupingNode.RowEvents.Select(rowEventInfo => new RowEventNodeViewModel(dacGroupingNode, rowEventInfo, ExpandCreatedNodes))
									 .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacGroupingNodeForCacheAttachedEventViewModel? dacGroupingNode) =>
			dacGroupingNode?.AllFieldEvents.Select(fieldEvent => new CacheAttachedNodeViewModel(dacGroupingNode, fieldEvent, ExpandCreatedNodes))
										   .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventViewModel dacGroupingNode) =>
			CreateDacChildrenForFieldEvents(dacGroupingNode,
				constructor: (dacVm, dacFieldName, dacFieldEvents) => new DacFieldGroupingNodeForFieldEventViewModel(dacVm, dacFieldName, dacFieldEvents, ExpandCreatedNodes));

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacChildrenForFieldEvents(DacGroupingNodeForFieldEventViewModel dacEventsGroupingNode,
																						 DacFieldVmConstructor constructor)
		{
			return from eventInfo in dacEventsGroupingNode.AllFieldEvents
				   group eventInfo by eventInfo.DacFieldName
						into dacFieldEvents
				   select constructor(dacEventsGroupingNode, dacFieldEvents.Key, dacFieldEvents)
						into dacFieldNodeVM
				   where !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   select dacFieldNodeVM;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacFieldGroupingNodeForFieldEventViewModel dacFieldGroupingNode) =>
			dacFieldGroupingNode?.FieldEvents.Select(fieldEvent => new FieldEventNodeViewModel(dacFieldGroupingNode, fieldEvent, ExpandCreatedNodes))
											 .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ViewNodeViewModel viewNode)
		{
			var hasViewDelegate = viewNode.MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(viewNode.MemberSymbol.Name,
																											  out DataViewDelegateInfo? viewDelegate);
			return hasViewDelegate
				? new GraphMemberInfoNodeViewModel(viewNode, viewDelegate!, GraphMemberInfoType.ViewDelegate, ExpandCreatedNodes).ToEnumerable()
				: DefaultValue;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ActionNodeViewModel actionNode)
		{
			var hasActionHandler =
				actionNode.MemberCategory.GraphSemanticModel.ActionHandlersByNames.TryGetValue(actionNode.MemberSymbol.Name,
																							   out ActionHandlerInfo? actionHandler);
			return hasActionHandler
				? new GraphMemberInfoNodeViewModel(actionNode, actionHandler!, GraphMemberInfoType.ActionHandler, ExpandCreatedNodes).ToEnumerable()
				: DefaultValue;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(CacheAttachedNodeViewModel cacheAttachedNode)
		{
			var attributes = cacheAttachedNode?.MemberSymbol.GetAttributes() ?? ImmutableArray<AttributeData>.Empty;

			if (attributes.IsDefaultOrEmpty)
				return [];

			return CreateCacheAttachedAttributeNodes();

			//-------------------------------------------Local Function------------------------------------------------------------------
			IEnumerable<CacheAttachedAttributeNodeViewModel> CreateCacheAttachedAttributeNodes()
			{
				// TODO add calculation of merge method later
				var defaultMergeMethod 	  = CacheAttachedAttributesMergeMethod.Replace;
				var graphSemanticModel 	  = cacheAttachedNode!.MemberCategory.GraphSemanticModel;
				var dbBoundnessCalculator = new DbBoundnessCalculator(graphSemanticModel.PXContext);

				for (int i = 0; i < attributes.Length; i++)
				{
					var attributeApplication = attributes[i];
					var attributeInfo = CacheAttachedAttributeInfo.Create(attributeApplication, defaultMergeMethod, dbBoundnessCalculator,
																		  declarationOrder: i);
					yield return new CacheAttachedAttributeNodeViewModel(cacheAttachedNode, attributeInfo, ExpandCreatedNodes);
				}
			}
		}
	}
}
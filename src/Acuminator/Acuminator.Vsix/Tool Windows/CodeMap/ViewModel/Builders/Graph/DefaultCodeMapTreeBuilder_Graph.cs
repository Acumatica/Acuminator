#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
using Acuminator.Vsix.ToolWindows.CodeMap.Graph;

using Microsoft.CodeAnalysis;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public partial class DefaultCodeMapTreeBuilder : TreeBuilderBase
	{
		protected delegate DacGroupingNodeBaseViewModel DacVmConstructor<THandlerInfo>(GraphEventHandlerCategoryNodeViewModel graphEventHandlerCategory,
																					   string dacName, IEnumerable<THandlerInfo> eventHandlerInfos)
		where THandlerInfo : GraphEventHandlerInfoBase<THandlerInfo>;

		protected delegate DacFieldGroupingNodeBaseViewModel DacFieldVmConstructor(DacGroupingNodeBaseViewModel dacNodeVm, string dacName,
																				   IEnumerable<GraphFieldEventHandlerInfo> fieldEventHandlers);
		protected virtual GraphNodeViewModel CreateGraphNode(GraphSemanticModelForCodeMap graph, TreeNodeViewModel? parent, TreeViewModel tree) =>
			new GraphNodeViewModel(graph, tree, parent, ExpandCreatedNodesCalculator);

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
			new GraphAttributesGroupNodeViewModel(graph.GraphSemanticModel, graph, ExpandCreatedNodesCalculator);

		protected virtual IEnumerable<GraphMemberCategory> GetGraphMemberTypesInOrder()
		{
			yield return GraphMemberCategory.BaseTypes;
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
				GraphMemberCategory.BaseTypes 					=> new GraphBaseTypesCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.InitializationAndActivation => new GraphInitializationAndActivationCategoryNodeViewModel(graph, parent: graph, 
																															 ExpandCreatedNodesCalculator),
				GraphMemberCategory.View 						=> new ViewCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.Action 						=> new ActionCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.CacheAttached 				=> new CacheAttachedCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.RowEvent 					=> new RowEventHandlerCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.FieldEvent 					=> new FieldEventHandlerCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.PXOverride 					=> new PXOverridesCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				GraphMemberCategory.BaseMemberOverride 			=> new GraphBaseMemberOverridesCategoryNodeViewModel(graph, parent: graph, ExpandCreatedNodesCalculator),
				_ 											=> null,
			};

		public override IEnumerable<TreeNodeViewModel> VisitNode(GraphAttributesGroupNodeViewModel attributeGroupNode) =>
			attributeGroupNode.AttributeInfos()
							  .Select(attrInfo => new GraphAttributeNodeViewModel(attributeGroupNode, attrInfo, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphInitializationAndActivationCategoryNodeViewModel graphInitializationAndActivationCategory)
		{
			return CreateGraphCategoryChildren<SymbolItem>(graphInitializationAndActivationCategory, InitializationAndActivationGraphMemberConstructor);

			//----------------------------------Local Function-----------------------------------------------------
			TreeNodeViewModel? InitializationAndActivationGraphMemberConstructor(SymbolItem symbolInfo) => symbolInfo switch
			{
				IsActiveMethodInfo isActiveMethodInfo 				  => new IsActiveGraphMethodNodeViewModel(graphInitializationAndActivationCategory,
																											  isActiveMethodInfo, ExpandCreatedNodesCalculator),
				InitializeMethodInfo initializeMethodInfo 			  => new GraphInitializeMethodNodeViewModel(graphInitializationAndActivationCategory,
																												initializeMethodInfo, ExpandCreatedNodesCalculator),
				StaticConstructorInfo staticConstructorInfo 		  => new GraphStaticConstructorNodeViewModel(graphInitializationAndActivationCategory,
																												 staticConstructorInfo, ExpandCreatedNodesCalculator),
				InstanceConstructorInfo instanceConstructorInfo 	  => new GraphInstanceConstructorNodeViewModel(graphInitializationAndActivationCategory,
																												   instanceConstructorInfo, ExpandCreatedNodesCalculator),
				ConfigureMethodInfo configureMethodInfo 			  => new GraphConfigureMethodNodeViewModel(graphInitializationAndActivationCategory, 
																											   configureMethodInfo, ExpandCreatedNodesCalculator),
				IsActiveForGraphMethodInfo isActiveForGraphMethodInfo => new IsActiveForGraphMethodNodeViewModel(graphInitializationAndActivationCategory,
																												 isActiveForGraphMethodInfo, ExpandCreatedNodesCalculator),
				_													  => null
			};
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphBaseTypesCategoryNodeViewModel graphBaseTypesCategory)
		{
			var baseTypesInfos = graphBaseTypesCategory?.GetCategoryGraphNodeSymbols();

			if (baseTypesInfos.IsNullOrEmpty())
				return DefaultValue;

			Cancellation.ThrowIfCancellationRequested();

			return baseTypesInfos.OfType<GraphOrGraphExtInfoBase>()
								 .Select(graphOrGraphExtInfo => new BaseGraphPlaceholderNodeViewModel(graphOrGraphExtInfo, graphBaseTypesCategory!.GraphViewModel,
																									  graphBaseTypesCategory, ExpandCreatedNodesCalculator));
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(GraphBaseMemberOverridesCategoryNodeViewModel graphBaseMemberOverridesCategory) =>
			CreateGraphCategoryChildren<BaseMemberOverrideInfo>(graphBaseMemberOverridesCategory,
						constructor: baseMemberOverrideInfo => new GraphBaseMemberOverrideNodeViewModel(graphBaseMemberOverridesCategory,
																										baseMemberOverrideInfo, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ActionCategoryNodeViewModel actionCategory) =>
			CreateGraphCategoryChildren<ActionInfo>(actionCategory,
						constructor: actionInfo => new ActionNodeViewModel(actionCategory, actionInfo, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ViewCategoryNodeViewModel viewCategory) =>
			 CreateGraphCategoryChildren<DataViewInfo>(viewCategory,
						constructor: viewInfo => new ViewNodeViewModel(viewCategory, viewInfo, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel>? VisitNode(PXOverridesCategoryNodeViewModel pxOverridesCategory) =>
			CreateGraphCategoryChildren<PXOverrideInfo>(pxOverridesCategory,
						constructor: pxOverrideInfo => new PXOverrideNodeViewModel(pxOverridesCategory, pxOverrideInfo, ExpandCreatedNodesCalculator));

		protected virtual IEnumerable<TreeNodeViewModel>? CreateGraphCategoryChildren<TSymbolInfo>(GraphMemberCategoryNodeViewModel graphMemberCategory,
																								   Func<TSymbolInfo, TreeNodeViewModel?> constructor)
		where TSymbolInfo : SymbolItem
		{
			var categoryInfos = graphMemberCategory?.GetCategoryGraphNodeSymbols();

			if (categoryInfos.IsNullOrEmpty())
				return DefaultValue;

			Cancellation.ThrowIfCancellationRequested();
			var graphSemanticModel = graphMemberCategory!.GraphSemanticModel;
			var graphMemberViewModels = from graphMemberInfo in categoryInfos.OfType<TSymbolInfo>()
										where graphMemberInfo.SymbolBase.IsDeclaredInType(graphSemanticModel.Symbol)
										select constructor(graphMemberInfo);

			return graphMemberViewModels;
		}

		public override IEnumerable<TreeNodeViewModel> VisitNode(RowEventHandlerCategoryNodeViewModel rowEventHandlerCategory) =>
			CreateEventHandlersCategoryChildren<GraphRowEventHandlerInfo>(rowEventHandlerCategory,
					(eventHandlerCategory, dacName, rowEventHandlers) => 
						new DacGroupingNodeForRowEventHandlerViewModel(eventHandlerCategory, dacName, rowEventHandlers, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel> VisitNode(CacheAttachedCategoryNodeViewModel cacheAttachedCategory) =>
			CreateEventHandlersCategoryChildren<GraphCacheAttachedEventHandlerInfo>(cacheAttachedCategory,
					(eventHandlerCategory, dacName, fieldEventHandlers) => 
						new DacGroupingNodeForCacheAttachedEventHandlerViewModel(eventHandlerCategory, dacName, fieldEventHandlers, ExpandCreatedNodesCalculator));

		public override IEnumerable<TreeNodeViewModel> VisitNode(FieldEventHandlerCategoryNodeViewModel fieldEventHandlerCategory) =>
			CreateEventHandlersCategoryChildren<GraphFieldEventHandlerInfo>(fieldEventHandlerCategory,
					(eventHandlerCategory, dacName, fieldEventHandlers) => 
						new DacGroupingNodeForFieldEventHandlerViewModel(eventHandlerCategory, dacName, fieldEventHandlers, ExpandCreatedNodesCalculator));

		protected virtual IEnumerable<TreeNodeViewModel> CreateEventHandlersCategoryChildren<THandlerInfo>(GraphEventHandlerCategoryNodeViewModel graphEventHandlersCategory,
																										   DacVmConstructor<THandlerInfo> constructor)
		where THandlerInfo : GraphEventHandlerInfoBase<THandlerInfo>
		{
			graphEventHandlersCategory.ThrowOnNull();

			var graphSemanticModel = graphEventHandlersCategory.GraphViewModel.GraphSemanticModel;
			var graphCategoryEventHandlers = graphEventHandlersCategory.GetCategoryGraphNodeSymbols()
																	  ?.OfType<THandlerInfo>()
																	   .Where(eventInfo => eventInfo.SignatureType != EventHandlerSignatureType.None);
			if (graphCategoryEventHandlers.IsNullOrEmpty())
				return [];

			Cancellation.ThrowIfCancellationRequested();
			var dacGroupingNodesViewModels = from handlerInfo in graphCategoryEventHandlers
											 group handlerInfo by handlerInfo.DacName into handlersForDac
											 select constructor(graphEventHandlersCategory, handlersForDac.Key, handlersForDac) into dacNodeVM
											 where dacNodeVM != null
											 select dacNodeVM;

			return dacGroupingNodesViewModels;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacGroupingNodeForRowEventHandlerViewModel dacGroupingNode) =>
			dacGroupingNode.RowEventHandlers
						   .Select(rowEventHandlerInfo => new RowEventHandlerNodeViewModel(dacGroupingNode, rowEventHandlerInfo, ExpandCreatedNodesCalculator))
						   .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacGroupingNodeForCacheAttachedEventHandlerViewModel? dacGroupingNode) =>
			dacGroupingNode?.AllFieldEventHandlers
							.Select(fieldEventHandler => new CacheAttachedNodeViewModel(dacGroupingNode, fieldEventHandler, ExpandCreatedNodesCalculator))
							.Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel> VisitNode(DacGroupingNodeForFieldEventHandlerViewModel dacGroupingNode) =>
			CreateDacChildrenForFieldEvents(dacGroupingNode,
				constructor: (dacVm, dacFieldName, dacFieldEventHandlers) => 
								new DacFieldGroupingNodeForFieldEventHandlerViewModel(dacVm, dacFieldName, dacFieldEventHandlers, ExpandCreatedNodesCalculator));

		protected virtual IEnumerable<TreeNodeViewModel> CreateDacChildrenForFieldEvents(DacGroupingNodeForFieldEventHandlerViewModel dacEventHandlersGroupingNode,
																						 DacFieldVmConstructor constructor)
		{
			return from handlerInfo in dacEventHandlersGroupingNode.AllFieldEventHandlers
				   group handlerInfo by handlerInfo.DacFieldName
						into dacFieldEventHandlers
				   select constructor(dacEventHandlersGroupingNode, dacFieldEventHandlers.Key, dacFieldEventHandlers)
						into dacFieldNodeVM
				   where !dacFieldNodeVM.DacFieldName.IsNullOrEmpty()
				   select dacFieldNodeVM;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(DacFieldGroupingNodeForFieldEventHandlerViewModel dacFieldGroupingNode) =>
			dacFieldGroupingNode?.FieldEventHandlers
								 .Select(fieldEventHandler => new FieldEventHandlerNodeViewModel(dacFieldGroupingNode, fieldEventHandler, ExpandCreatedNodesCalculator))
								 .Where(graphMemberVM => !graphMemberVM.Name.IsNullOrEmpty());

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ViewNodeViewModel viewNode)
		{
			var hasViewDelegate = viewNode.MemberCategory.GraphSemanticModel.ViewDelegatesByNames.TryGetValue(viewNode.MemberSymbol.Name,
																											  out DataViewDelegateInfo? viewDelegate);
			return hasViewDelegate
				? new GraphMemberInfoNodeViewModel(viewNode, viewDelegate!, GraphMemberInfoType.ViewDelegate, ExpandCreatedNodesCalculator).ToEnumerable()
				: DefaultValue;
		}

		public override IEnumerable<TreeNodeViewModel>? VisitNode(ActionNodeViewModel actionNode)
		{
			var hasActionDelegate =
				actionNode.MemberCategory.GraphSemanticModel.ActionDelegateByNames.TryGetValue(actionNode.MemberSymbol.Name,
																							   out ActionDelegateInfo? actionDelegate);
			return hasActionDelegate
				? new GraphMemberInfoNodeViewModel(actionNode, actionDelegate!, GraphMemberInfoType.ActionDelegate, ExpandCreatedNodesCalculator).ToEnumerable()
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
					yield return new CacheAttachedAttributeNodeViewModel(cacheAttachedNode, attributeInfo, ExpandCreatedNodesCalculator);
				}
			}
		}
	}
}
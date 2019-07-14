using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacSemanticModel
	{
		private readonly CancellationToken _cancellation;
		private readonly PXContext _pxContext;

		public DacType DacType { get; }

		public INamedTypeSymbol Symbol { get; }

		/// <summary>
		/// The DAC symbol. For the DAC is the same as <see cref="Symbol"/>. For DAC extensions is the extension's base DAC.
		/// </summary>
		public ITypeSymbol DacSymbol { get; }

		public ImmutableDictionary<string, DacPropertyInfo> PropertiesByNames { get; }
		public IEnumerable<DacPropertyInfo> Properties => PropertiesByNames.Values;

		public ImmutableDictionary<string, DacFieldInfo> FieldsByNames { get; }
		public IEnumerable<DacFieldInfo> Fields => FieldsByNames.Values;

		private DacSemanticModel(PXContext pxContext, DacType dacType, INamedTypeSymbol symbol,
								 CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			_pxContext = pxContext;
			DacType = dacType;
			Symbol = symbol;
			_cancellation = cancellation;

			switch (DacType)
			{
				case DacType.Dac:
					DacSymbol = Symbol;
					break;
				case DacType.DacExtension:
					DacSymbol = Symbol.GetDacFromDacExtension(_pxContext);
					break;
				case DacType.None:
				default:
					DacSymbol = null;
					break;
			}

			//StaticConstructors = Symbol.GetStaticConstructors(_cancellation);
			//ViewsByNames = GetDataViews();
			//ViewDelegatesByNames = GetDataViewDelegates();

			//ActionsByNames = GetActions();
			//ActionHandlersByNames = GetActionHandlers();

			//InitProcessingDelegatesInfo();
			//InitDeclaredInitializers();
		}

		private ImmutableDictionary<string, DacPropertyInfo> GetDacProperties()
		{
			if (DacType == DacType.None)
				return ImmutableDictionary.Create<string, DacPropertyInfo>(StringComparer.OrdinalIgnoreCase);

			var rawPropertyInfos = DacType == DacType.Dac
				? Symbol.GetDacPropertySymbolsWithNodesFromDac(_pxContext)
				: Symbol.GetPropertiesFromDacExtensionAndBaseDac(_pxContext);

			return rawPropertyInfos.ToLookup(a => a.Item.DacProperty.Name, StringComparer.OrdinalIgnoreCase)
								   .ToImmutableDictionary(group => group.Key,
														  group => CreateViewInfo(group.First()),
														  keyComparer: StringComparer.OrdinalIgnoreCase);


			DataViewInfo CreateDacPropertyInfo(GraphOverridableItem<(ISymbol ViewSymbol, INamedTypeSymbol ViewType)> item)
			{
				var (viewSymbol, viewType) = item.Item;

				DataViewInfo baseViewInfo = item.Base != null
					? CreateViewInfo(item.Base)
					: null;

				return baseViewInfo == null
					? new DataViewInfo(viewSymbol, viewType, _pxContext, item.DeclarationOrder)
					: new DataViewInfo(viewSymbol, viewType, _pxContext, item.DeclarationOrder, baseViewInfo);
			}
		}

		//private ImmutableDictionary<string, DataViewDelegateInfo> GetDataViewDelegates()
		//{
		//	if (Type == GraphType.None)
		//		return ImmutableDictionary.Create<string, DataViewDelegateInfo>(StringComparer.OrdinalIgnoreCase);

		//	var rawDelegateInfos = Type == GraphType.PXGraph
		//		? Symbol.GetViewDelegatesFromGraph(ViewsByNames, _pxContext, _cancellation)
		//		: Symbol.GetViewDelegatesFromGraphExtensionAndBaseGraph(ViewsByNames, _pxContext, _cancellation);

		//	return rawDelegateInfos.ToLookup(d => d.Item.Symbol.Name, StringComparer.OrdinalIgnoreCase)
		//						   .ToImmutableDictionary(group => group.Key,
		//												  group => CreateViewDelegateInfo(group.First()),
		//												  keyComparer: StringComparer.OrdinalIgnoreCase);


		//	DataViewDelegateInfo CreateViewDelegateInfo(GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)> item)
		//	{
		//		var (node, method) = item.Item;

		//		DataViewDelegateInfo baseDelegateInfo = item.Base != null
		//			? CreateViewDelegateInfo(item.Base)
		//			: null;

		//		return baseDelegateInfo == null
		//			? new DataViewDelegateInfo(node, method, item.DeclarationOrder)
		//			: new DataViewDelegateInfo(node, method, item.DeclarationOrder, baseDelegateInfo);
		//	}
		//}

		//private ImmutableDictionary<string, ActionInfo> GetActions()
		//{
		//	if (Type == GraphType.None)
		//		return ImmutableDictionary.Create<string, ActionInfo>(StringComparer.OrdinalIgnoreCase);

		//	var systemActionsRegister = new PXSystemActions.PXSystemActionsRegister(_pxContext);
		//	var rawActionInfos = Type == GraphType.PXGraph
		//		? Symbol.GetActionSymbolsWithTypesFromGraph(_pxContext)
		//		: Symbol.GetActionsFromGraphExtensionAndBaseGraph(_pxContext);

		//	return rawActionInfos.ToLookup(a => a.Item.ActionSymbol.Name, StringComparer.OrdinalIgnoreCase)
		//						 .ToImmutableDictionary(group => group.Key,
		//												group => CreateActionInfo(group.First()),
		//												keyComparer: StringComparer.OrdinalIgnoreCase);



		//	ActionInfo CreateActionInfo(GraphOverridableItem<(ISymbol ActionSymbol, INamedTypeSymbol ActionType)> item)
		//	{
		//		var (actionSymbol, actionType) = item.Item;

		//		ActionInfo baseActionInfo = item.Base != null
		//			? CreateActionInfo(item.Base)
		//			: null;

		//		return baseActionInfo == null
		//			? new ActionInfo(actionSymbol, actionType, item.DeclarationOrder, systemActionsRegister.IsSystemAction(actionType))
		//			: new ActionInfo(actionSymbol, actionType, item.DeclarationOrder,
		//							 systemActionsRegister.IsSystemAction(actionType), baseActionInfo);
		//	}
		//}

		//private ImmutableDictionary<string, ActionHandlerInfo> GetActionHandlers()
		//{
		//	if (Type == GraphType.None)
		//		return ImmutableDictionary.Create<string, ActionHandlerInfo>(StringComparer.OrdinalIgnoreCase);

		//	var rawActionHandlerInfos = Type == GraphType.PXGraph
		//		? Symbol.GetActionHandlersFromGraph(ActionsByNames, _pxContext, _cancellation)
		//		: Symbol.GetActionHandlersFromGraphExtensionAndBaseGraph(ActionsByNames, _pxContext, _cancellation);

		//	return rawActionHandlerInfos.ToLookup(handler => handler.Item.Symbol.Name, StringComparer.OrdinalIgnoreCase)
		//								.ToImmutableDictionary(group => group.Key,
		//													   group => CreateActionHandlerInfo(group.First()),
		//													   keyComparer: StringComparer.OrdinalIgnoreCase);

		//	ActionHandlerInfo CreateActionHandlerInfo(GraphOverridableItem<(MethodDeclarationSyntax, IMethodSymbol)> item)
		//	{
		//		var (node, method) = item.Item;

		//		ActionHandlerInfo baseActionHandlerInfo = item.Base != null
		//			? CreateActionHandlerInfo(item.Base)
		//			: null;

		//		return baseActionHandlerInfo == null
		//			? new ActionHandlerInfo(node, method, item.DeclarationOrder)
		//			: new ActionHandlerInfo(node, method, item.DeclarationOrder, baseActionHandlerInfo);
		//	}
		//}

		/// <summary>
		/// Returns semantic model of DAC or DAC Extension which is inferred from <paramref name="typeSymbol"/>
		/// </summary>
		/// <param name="pxContext">Context instance</param>
		/// <param name="typeSymbol">Symbol which is DAC or DAC Extension descendant</param>
		/// <param name="semanticModel">Semantic model</param>
		/// <param name="cancellation">Cancellation</param>
		/// <returns/>
		public static DacSemanticModel InferModel(PXContext pxContext, INamedTypeSymbol typeSymbol, CancellationToken cancellation = default)
		{
			throw new NotImplementedException();
			//cancellation.ThrowIfCancellationRequested();
			//pxContext.ThrowOnNull(nameof(pxContext));
			//typeSymbol.ThrowOnNull(nameof(typeSymbol));

			//var models = new List<PXGraphSemanticModel>();
			//var explicitModel = InferExplicitModel(pxContext, typeSymbol, cancellation);

			//if (explicitModel != null)
			//{
			//	models.Add(explicitModel);
			//}

			//InferImplicitModels(pxContext, typeSymbol, models, cancellation);

			//return models;
		}

		//private static void InferImplicitModels(PXContext pxContext, INamedTypeSymbol typeSymbol,
		//										List<PXGraphSemanticModel> models, CancellationToken cancellation)
		//{
		//	cancellation.ThrowIfCancellationRequested();

		//	IEnumerable<InitDelegateInfo> delegates = GetInitDelegates(pxContext, typeSymbol, cancellation);

		//	foreach (InitDelegateInfo d in delegates)
		//	{
		//		GraphInitializerInfo info = new GraphInitializerInfo(GraphInitializerType.InstanceCreatedDelegate, d.Node,
		//															 d.Symbol, d.DeclarationOrder);
		//		PXGraphSemanticModel existingModel = models.FirstOrDefault(m => m.Symbol.Equals(d.GraphTypeSymbol));
		//		PXGraphSemanticModel implicitModel;

		//		if (existingModel != null)
		//		{
		//			implicitModel = existingModel;
		//		}
		//		else
		//		{
		//			implicitModel = new PXGraphSemanticModel(pxContext, d.GraphType, d.GraphTypeSymbol, cancellation);
		//			models.Add(implicitModel);
		//		}

		//		implicitModel.Initializers = implicitModel.Initializers.Add(info);
		//	}
		//}

		//public static PXGraphSemanticModel InferExplicitModel(PXContext pxContext, INamedTypeSymbol typeSymbol,
		//	CancellationToken cancellation)
		//{
		//	cancellation.ThrowIfCancellationRequested();
		//	pxContext.ThrowOnNull(nameof(pxContext));
		//	typeSymbol.ThrowOnNull(nameof(typeSymbol));

		//	GraphType graphType = GraphType.None;

		//	if (typeSymbol.IsPXGraph(pxContext))
		//	{
		//		graphType = GraphType.PXGraph;
		//	}
		//	else if (typeSymbol.IsPXGraphExtension(pxContext))
		//	{
		//		graphType = GraphType.PXGraphExtension;
		//	}

		//	if (graphType != GraphType.None)
		//	{
		//		return new PXGraphSemanticModel(pxContext, graphType, typeSymbol, cancellation);
		//	}

		//	return null;
		//}

		//private static IEnumerable<InitDelegateInfo> GetInitDelegates(PXContext pxContext, INamedTypeSymbol typeSymbol,
		//															  CancellationToken cancellation)
		//{
		//	cancellation.ThrowIfCancellationRequested();

		//	IEnumerable<SyntaxNode> declaringNodes = typeSymbol.DeclaringSyntaxReferences
		//													   .Select(r => r.GetSyntax(cancellation));
		//	InstanceCreatedEventsAddHandlerWalker walker = new InstanceCreatedEventsAddHandlerWalker(pxContext, cancellation);

		//	foreach (SyntaxNode node in declaringNodes)
		//	{
		//		cancellation.ThrowIfCancellationRequested();
		//		walker.Visit(node);
		//	}

		//	return walker.GraphInitDelegates;
		//}
	}
}

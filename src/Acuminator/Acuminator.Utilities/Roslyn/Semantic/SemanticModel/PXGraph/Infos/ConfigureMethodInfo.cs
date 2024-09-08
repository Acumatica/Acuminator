using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph;

/// <summary>
/// Information about the Configure special method in graph extensions.
/// </summary>
public class ConfigureMethodInfo : NodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>, IWriteableBaseItem<ConfigureMethodInfo>
{
	/// <summary>
	/// The Configure method declaration order to place it fourth after the Initialize method.
	/// </summary>
	internal const int ConfigureDeclarationOrderToPlaceItFourth = InitializeMethodInfo.InitializeMethodDeclarationOrderToPlaceItThird + 1;

	protected ConfigureMethodInfo? _baseInfo;

	/// <summary>
	/// The overriden Configure method if any.
	/// </summary>
	public ConfigureMethodInfo? Base => _baseInfo;

	ConfigureMethodInfo? IWriteableBaseItem<ConfigureMethodInfo>.Base
	{
		get => Base;
		set 
		{
			_baseInfo = value;

			if (value != null)
				CombineWithBaseInfo(value);
		}
	}

	public ConfigureMethodInfo(MethodDeclarationSyntax? configureMethodNode, IMethodSymbol configureMethod, int declarationOrder, 
							   ConfigureMethodInfo baseInfo) : 
						  this(configureMethodNode, configureMethod, declarationOrder)
	{
		_baseInfo = baseInfo.CheckIfNull();
		CombineWithBaseInfo(_baseInfo);
	}

	public ConfigureMethodInfo(MethodDeclarationSyntax? configureMethodNode, IMethodSymbol configureMethod, int declarationOrder) : 
						  base(configureMethodNode, configureMethod, declarationOrder)
	{
	}

	public void CombineWithBaseInfo(ConfigureMethodInfo baseInfo)
	{
	}

	/// <summary>
	/// Collects info about Configure method overrides from a graph or graph extension symbol and creates a <see cref="ConfigureMethodInfo"/> DTO.
	/// </summary>
	/// <remarks>
	/// We collect only Configure method overrides from the class hierarchy because:<br/>
	/// - PXOverride mechanism is not supported by the workflow mechanism. Thus, it's no use to check chained extensions, <br/>  
	/// they can't affect workflow configuration done by the base extension or graph.<br/>
	/// - Graph and graph extension overrides of Configure method can be considered independently.<br/>  
	/// Therefore, for graph extension base graph's Configure method overrides are not included into results.<br/>
	/// Thus, only one <see cref="ConfigureMethodInfo"/> will be created in the end. The created DTO will contain all Configure method overrides as base infos.<br/>
	/// The created <see cref="ConfigureMethodInfo"/> DTO is not neccessary declared in the <paramref name="graphOrGraphExtension"/> symbol. <br/>
	/// It can be also declared in its base types.
	/// </remarks>
	/// <param name="graphOrGraphExtension">The graph or graph extension.</param>
	/// <param name="graphType">Type of the <paramref name="graphOrGraphExtension"/> symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
	/// <param name="customDeclarationOrder">(Optional) The declaration order. Default value is <see cref="ConfigureDeclarationOrderToPlaceItThird"/>.</param>
	/// <returns>
	/// <see cref="ConfigureMethodInfo"/> DTO if the graph / graph extension contains one Configure method, otherwise <see langword="null"/>.
	/// </returns>
	internal static ConfigureMethodInfo? GetConfigureMethodInfo(INamedTypeSymbol graphOrGraphExtension, GraphType graphType, PXContext pxContext, 
																CancellationToken cancellationToken, int? customDeclarationOrder = null)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var originalConfigureMethod = GetOriginalConfigureMethod(graphType, pxContext);

		if (originalConfigureMethod == null)
			return null;

		int startDeclarationOrder = customDeclarationOrder ?? ConfigureDeclarationOrderToPlaceItFourth;
		var graphOrExtTypeHierarchyFromDerivedToBase = graphOrGraphExtension.GetBaseTypesAndThis();

		foreach (ITypeSymbol graphOrExtType in graphOrExtTypeHierarchyFromDerivedToBase)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var configureCandidates = GetConfigureMethodCandidatesInType(graphOrExtType).ToList(capacity: 2);

			if (configureCandidates.Count == 0)
				continue;

			ConfigureMethodInfo? configureMethodInfo = TryGetConfigureMethodInfo(configureCandidates, originalConfigureMethod,
																				 startDeclarationOrder, cancellationToken);
			if (configureMethodInfo != null)
				return configureMethodInfo;
		}

		return null;
	}

	private static IMethodSymbol? GetOriginalConfigureMethod(GraphType graphType, PXContext pxContext) =>
		graphType switch
		{
			GraphType.PXGraph 		   => pxContext?.PXGraph.Configure,
			GraphType.PXGraphExtension => pxContext?.PXGraphExtension.Configure,
			_ 						   => null
		};

	private static IEnumerable<IMethodSymbol> GetConfigureMethodCandidatesInType(ITypeSymbol graphOrExtType)
	{
		var allConfigureMethodsInType = graphOrExtType.GetMethods(DelegateNames.Workflow.Configure);
		var configureCandidates = from method in allConfigureMethodsInType
								  where !method.IsStatic && method.ReturnsVoid && method.IsOverride &&
										 method.DeclaredAccessibility == Accessibility.Public && method.Parameters.Length == 1 &&
										 method.IsDeclaredInType(graphOrExtType)
								  select method;
		return configureCandidates;
	}

	private static ConfigureMethodInfo? TryGetConfigureMethodInfo(List<IMethodSymbol> configureCandidatesInType, IMethodSymbol originalConfigureMethod,
																  int startDeclarationOrder, CancellationToken cancellation)
	{
		foreach (IMethodSymbol configureMethodCandidate in configureCandidatesInType)
		{
			cancellation.ThrowIfCancellationRequested();
			var overridesChainFromBaseToDerived = configureMethodCandidate.GetOverriddenAndThis()
																		  .Reverse()
																		  .ToList(capacity: 4);
			var originalVirtualMethod = overridesChainFromBaseToDerived[0];

			if (!originalConfigureMethod.Equals(originalVirtualMethod, SymbolEqualityComparer.Default) || overridesChainFromBaseToDerived.Count <= 1)
				continue;

			// Do not include the original PXGraphExtension.Configure method into results
			var configureMethodOverrides = overridesChainFromBaseToDerived.Skip(1);
			var configureMethodInfo = GetConfigureMethodInfoFromOverridesChain(configureMethodOverrides, startDeclarationOrder, cancellation);

			return configureMethodInfo;
		}

		return null;
	}

	private static ConfigureMethodInfo? GetConfigureMethodInfoFromOverridesChain(IEnumerable<IMethodSymbol> configureMethodOverrides,
																				int startDeclarationOrder, CancellationToken cancellation)
	{
		ConfigureMethodInfo? baseInfo = null;
		ConfigureMethodInfo? current = null;

		foreach (var configureOverride in configureMethodOverrides)
		{
			var configureMethodOverrideNode = configureOverride.GetSyntax(cancellation) as MethodDeclarationSyntax;
			current = baseInfo != null
				? new ConfigureMethodInfo(configureMethodOverrideNode, configureOverride, startDeclarationOrder, baseInfo)
				: new ConfigureMethodInfo(configureMethodOverrideNode, configureOverride, startDeclarationOrder);

			baseInfo = current;
		}

		return current;
	}
}
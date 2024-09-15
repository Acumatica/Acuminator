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
/// Information about the Initialize special method in graph extensions.
/// </summary>
public class InitializeMethodInfo : OverridableNodeSymbolItem<InitializeMethodInfo, MethodDeclarationSyntax, IMethodSymbol>
{
	/// <summary>
	/// The Initialize method declaration order to place it third after IsActiveForGraph method.
	/// </summary>
	internal const int InitializeMethodDeclarationOrderToPlaceItThird = IsActiveForGraphMethodInfo.IsActiveForGraphDeclarationOrderToPlaceItSecond + 1;

	public InitializeMethodInfo(MethodDeclarationSyntax? initializeMethodNode, IMethodSymbol initializeMethod, int declarationOrder,
							    InitializeMethodInfo baseInfo) :
						  base(initializeMethodNode, initializeMethod, declarationOrder, baseInfo)
	{
	}

	public InitializeMethodInfo(MethodDeclarationSyntax? initializeMethodNode, IMethodSymbol initializeMethod, int declarationOrder) :
						  base(initializeMethodNode, initializeMethod, declarationOrder)
	{
	}

	/// <summary>
	/// Collects info about Initialize method overrides from a graph or graph extension symbol and creates a <see cref="InitializeMethodInfo"/> DTO.
	/// </summary>
	/// <remarks>
	/// We collect only Initialize method overrides from the class hierarchy because graph and graph extension overrides of Initialize method can be considered independent.<br/>
	/// Therefore, for graph extension base graph's Initialize method overrides are not included into results.<br/>
	/// Thus, only one <see cref="InitializeMethodInfo"/> will be created in the end. The created DTO will contain all Initialize method overrides as base infos.<br/>
	/// The created <see cref="InitializeMethodInfo"/> DTO is not neccessary declared in the <paramref name="graphOrGraphExtension"/> symbol. <br/>
	/// It can be also declared in its base types.
	/// </remarks>
	/// <param name="graphOrGraphExtension">The graph or graph extension.</param>
	/// <param name="graphType">Type of the <paramref name="graphOrGraphExtension"/> symbol.</param>
	/// <param name="pxContext">The Acumatica context.</param>
	/// <param name="cancellationToken">A token that allows processing to be cancelled.</param>
	/// <param name="customDeclarationOrder">(Optional) The declaration order. Default value is <see cref="ConfigureDeclarationOrderToPlaceItThird"/>.</param>
	/// <returns>
	/// <see cref="InitializeMethodInfo"/> DTO if the graph / graph extension contains Initialize method, otherwise <see langword="null"/>.
	/// </returns>
	internal static InitializeMethodInfo? GetInitializeMethodInfo(INamedTypeSymbol graphOrGraphExtension, GraphType graphType, PXContext pxContext,
																  CancellationToken cancellationToken, int? customDeclarationOrder = null)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var originalInitializeMethod = GetOriginalInitializeMethod(graphType, graphOrGraphExtension, pxContext);

		if (originalInitializeMethod == null)
			return null;

		int startDeclarationOrder = customDeclarationOrder ?? InitializeMethodDeclarationOrderToPlaceItThird;
		var graphOrExtTypeHierarchyFromDerivedToBase = graphOrGraphExtension.GetBaseTypesAndThis();

		foreach (ITypeSymbol graphOrExtType in graphOrExtTypeHierarchyFromDerivedToBase)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var initializeCandidates = GetInitializeMethodCandidatesInType(graphOrExtType, graphType).ToList(capacity: 2);

			if (initializeCandidates.Count == 0)
				continue;

			InitializeMethodInfo? initializeMethodInfo = TryGetInitializeMethodInfo(initializeCandidates, originalInitializeMethod,
																					startDeclarationOrder, graphType, cancellationToken);
			if (initializeMethodInfo != null)
				return initializeMethodInfo;
		}

		return null;
	}

	private static IMethodSymbol? GetOriginalInitializeMethod(GraphType graphType, INamedTypeSymbol graphOrGraphExtension, PXContext pxContext)
	{
		switch (graphType)
		{
			case GraphType.PXGraph:
				var iGraphWithInitialization = pxContext.IGraphWithInitializationSymbols.Type;
				if (iGraphWithInitialization == null || !graphOrGraphExtension.ImplementsInterface(iGraphWithInitialization))
					return null;

				return pxContext.IGraphWithInitializationSymbols.Initialize;

			case GraphType.PXGraphExtension:
				return pxContext.PXGraphExtension.Initialize;

			default:
				return null;
		}
	}

	private static IEnumerable<IMethodSymbol> GetInitializeMethodCandidatesInType(ITypeSymbol graphOrExtType, GraphType graphType)
	{
		if (graphType == GraphType.PXGraph)
		{
			return from method in graphOrExtType.GetMethods()
				   where method.IsValidInitializeMethod() && method.IsDeclaredInType(graphOrExtType) &&
						(IsImplicitInitializeMethodImplementation(method) || IsExplicitInitializeMethodImplementation(method))
				   select method;
		}
		else
		{
			return from method in graphOrExtType.GetMethods(DelegateNames.Initialize)
				   where method.IsValidInitializeMethod() && method.IsDeclaredInType(graphOrExtType) &&
						 method.IsOverride && method.DeclaredAccessibility == Accessibility.Public
				   select method;
		}

		//-------------------------------------------Local Functions-----------------------------------------------------
		static bool IsImplicitInitializeMethodImplementation(IMethodSymbol method) =>
			method.MethodKind == MethodKind.Ordinary && method.Name == DelegateNames.Initialize &&
			method.DeclaredAccessibility == Accessibility.Public;
		//--------------------------------------------------------------------------------------------------------------
		static bool IsExplicitInitializeMethodImplementation(IMethodSymbol method) =>
			method.MethodKind == MethodKind.ExplicitInterfaceImplementation && LastSegmentIsInitialize(method.Name);
		//--------------------------------------------------------------------------------------------------------------
		static bool LastSegmentIsInitialize(string methodName)
		{
			int lastDotIndex = methodName.LastIndexOf('.');

			if (lastDotIndex < 0)
				return methodName == DelegateNames.Initialize;

			var methodNameSpan = methodName.AsSpan();
			var lastSegment = methodNameSpan[(lastDotIndex + 1)..];

			return lastSegment.Equals(DelegateNames.Initialize.AsSpan(), StringComparison.Ordinal);
		}
	}

	private static InitializeMethodInfo? TryGetInitializeMethodInfo(List<IMethodSymbol> initializeCandidatesInType, IMethodSymbol originalInitializeMethod,
																   int startDeclarationOrder, GraphType graphType, CancellationToken cancellation)
	{
		foreach (IMethodSymbol initializeMethodCandidate in initializeCandidatesInType)
		{
			cancellation.ThrowIfCancellationRequested();
			var overridesChainFromBaseToDerived = initializeMethodCandidate.GetOverriddenAndThis()
																		   .Reverse()
																		   .ToList(capacity: 4);
			var firstMethodInChain = overridesChainFromBaseToDerived[0];

			if (graphType == GraphType.PXGraphExtension)
			{
				if (!originalInitializeMethod.Equals(firstMethodInChain, SymbolEqualityComparer.Default) || overridesChainFromBaseToDerived.Count <= 1)
					continue;

				// Do not include the original PXGraphExtension.Initialize method into results
				var initializeMethodOverridesInGraphExt = overridesChainFromBaseToDerived.Skip(1);
				var initializeMethodInfo = GetInitializeMethodInfoFromOverridesChain(initializeMethodOverridesInGraphExt, startDeclarationOrder,
																					 cancellation);
				return initializeMethodInfo;
			}
			else
			{
				if (!IsInitializeMethodImplementationInGraph(overridesChainFromBaseToDerived, originalInitializeMethod))
					continue;

				var initializeMethodInfo = GetInitializeMethodInfoFromOverridesChain(overridesChainFromBaseToDerived, startDeclarationOrder,
																					 cancellation);
				return initializeMethodInfo;
			}
		}

		return null;
	}

	private static bool IsInitializeMethodImplementationInGraph(List<IMethodSymbol> overridesChainFromBaseToDerived, 
																IMethodSymbol originalInitializeMethod)
	{
		var firstMethodInChain = overridesChainFromBaseToDerived[0];

		if (!firstMethodInChain.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
		{
			return firstMethodInChain.ExplicitInterfaceImplementations
									 .Any(method => method.Equals(originalInitializeMethod, SymbolEqualityComparer.Default));
		} 

		var initializeMethodImplementation = firstMethodInChain.ContainingType.FindImplementationForInterfaceMember(originalInitializeMethod);

		if (initializeMethodImplementation == null)
			return false;
		else if (SymbolEqualityComparer.Default.Equals(initializeMethodImplementation, firstMethodInChain))
			return true;

		return overridesChainFromBaseToDerived.Skip(1)
											  .Contains(initializeMethodImplementation, SymbolEqualityComparer.Default);
	}

	private static InitializeMethodInfo? GetInitializeMethodInfoFromOverridesChain(IEnumerable<IMethodSymbol> initializeMethodOverrides,
																					int startDeclarationOrder, CancellationToken cancellation)
	{
		InitializeMethodInfo? baseInfo = null;
		InitializeMethodInfo? current = null;

		foreach (var initializeMethodOverride in initializeMethodOverrides)
		{
			var initializeMethodOverrideNode = initializeMethodOverride.GetSyntax(cancellation) as MethodDeclarationSyntax;
			current = baseInfo != null
				? new InitializeMethodInfo(initializeMethodOverrideNode, initializeMethodOverride, startDeclarationOrder, baseInfo)
				: new InitializeMethodInfo(initializeMethodOverrideNode, initializeMethodOverride, startDeclarationOrder);

			baseInfo = current;
		}

		return current;
	}
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.BannedApi.ApiInfoRetrievers;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Analyzers.StaticAnalysis.BannedApi;

public partial class BannedApiAnalyzer
{
	private class BannedTypesInfoCollector
	{
		private readonly CancellationToken _cancellation;
		private readonly IApiInfoRetriever _apiBanInfoRetriever;
		private readonly IApiInfoRetriever? _allowedInfoRetriever;
		private readonly HashSet<ITypeSymbol> _checkedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

		public HashSet<string> NamespacesWithUsedWhiteListedMembers { get; } = new();

		public BannedTypesInfoCollector(IApiInfoRetriever apiBanInfoRetriever, IApiInfoRetriever? allowedInfoRetriever,
										CancellationToken cancellation)
		{
			_apiBanInfoRetriever  = apiBanInfoRetriever;
			_allowedInfoRetriever = allowedInfoRetriever;
			_cancellation = cancellation;
		}

		public List<ApiSearchResult>? GetTypeParameterBannedApiInfos(ITypeParameterSymbol typeParameterSymbol, bool checkInterfaces)
		{
			_checkedTypes.Clear();
			return GetBannedInfosFromTypeParameter(typeParameterSymbol, alreadyCollectedInfos: null, checkInterfaces);
		}

		public List<ApiSearchResult>? GetTypeBannedApiInfos(ITypeSymbol typeSymbol, bool checkInterfaces)
		{
			_checkedTypes.Clear();

			var typeSymbolToAnalize = GetUnderlyingTypeSymbolForAnalysis(typeSymbol);
			return GetBannedInfosFromTypeSymbolAndItsHierarchy(typeSymbolToAnalize, alreadyCollectedInfos: null, checkInterfaces);
		}

		private ITypeSymbol GetUnderlyingTypeSymbolForAnalysis(ITypeSymbol typeSymbol) =>
			typeSymbol switch
			{
				IPointerTypeSymbol pointerTypeSymbol => pointerTypeSymbol.PointedAtType,
				IArrayTypeSymbol arrayTypeSymbol	 => arrayTypeSymbol.ElementType,
				_ 									 => typeSymbol
			};

		private List<ApiSearchResult>? GetBannedInfosFromTypeSymbolAndItsHierarchy(ITypeSymbol typeSymbol, List<ApiSearchResult>? alreadyCollectedInfos,
																				   bool checkInterfaces)
		{
			if (!_checkedTypes.Add(typeSymbol))
				return alreadyCollectedInfos;

			_cancellation.ThrowIfCancellationRequested();

			if (typeSymbol.SpecialType != SpecialType.None)
				return alreadyCollectedInfos;

			alreadyCollectedInfos = GetBannedInfosFromType(typeSymbol, alreadyCollectedInfos, checkInterfaces);
			alreadyCollectedInfos = GetBannedInfosFromBaseTypes(typeSymbol, alreadyCollectedInfos, checkInterfaces);

			_cancellation.ThrowIfCancellationRequested();

			if (!checkInterfaces)
				return alreadyCollectedInfos;

			var interfaces = typeSymbol.AllInterfaces;

			if (interfaces.IsDefaultOrEmpty)
				return alreadyCollectedInfos;

			foreach (INamedTypeSymbol @interface in interfaces)
			{
				if (_checkedTypes.Add(@interface))
				{
					_cancellation.ThrowIfCancellationRequested();
					alreadyCollectedInfos = GetBannedInfosFromType(@interface, alreadyCollectedInfos, checkInterfaces);
				}
			}

			_cancellation.ThrowIfCancellationRequested();
			return alreadyCollectedInfos;
		}

		private List<ApiSearchResult>? GetBannedInfosFromBaseTypes(ITypeSymbol typeSymbol, List<ApiSearchResult>? alreadyCollectedInfos, bool checkInterfaces)
		{
			if (typeSymbol.IsStatic || typeSymbol.TypeKind != TypeKind.Class)
				return alreadyCollectedInfos;

			foreach (var baseType in typeSymbol.GetBaseTypes())
			{
				_cancellation.ThrowIfCancellationRequested();

				if (!_checkedTypes.Add(baseType) || baseType.SpecialType != SpecialType.None)
					return alreadyCollectedInfos;

				int oldCount = alreadyCollectedInfos?.Count ?? 0;
				alreadyCollectedInfos = GetBannedInfosFromType(typeSymbol, alreadyCollectedInfos, checkInterfaces);

				// If we found something incompatible there is no need to go lower. We don't need to report whole incompatible inheritance chain
				int newCount = alreadyCollectedInfos?.Count ?? 0;

				if (oldCount != newCount)
					return alreadyCollectedInfos;
			}

			return alreadyCollectedInfos;
		}

		private List<ApiSearchResult>? GetBannedInfosFromType(ITypeSymbol typeSymbol, List<ApiSearchResult>? alreadyCollectedInfos, bool checkInterfaces)
		{
			if (_apiBanInfoRetriever.GetInfoForApi(typeSymbol) is ApiSearchResult bannedTypeInfo && !IsInWhiteList(typeSymbol))
			{
				alreadyCollectedInfos ??= new List<ApiSearchResult>(capacity: 4);
				alreadyCollectedInfos.Add(bannedTypeInfo);
			}

			_cancellation.ThrowIfCancellationRequested();

			if (typeSymbol is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType)
				alreadyCollectedInfos = GetBannedApisFromTypesList(namedTypeSymbol.TypeArguments, alreadyCollectedInfos, checkInterfaces);

			return alreadyCollectedInfos;
		}

		private List<ApiSearchResult>? GetBannedInfosFromTypeParameter(ITypeParameterSymbol typeParameterSymbol, List<ApiSearchResult>? alreadyCollectedInfos,
																 bool checkInterfaces)
		{
			if (!_checkedTypes.Add(typeParameterSymbol))
				return alreadyCollectedInfos;

			return GetBannedApisFromTypesList(typeParameterSymbol.ConstraintTypes, alreadyCollectedInfos, checkInterfaces);
		}

		private List<ApiSearchResult>? GetBannedApisFromTypesList(ImmutableArray<ITypeSymbol> types, List<ApiSearchResult>? alreadyCollectedInfos, bool checkInterfaces)
		{
			if (types.IsDefaultOrEmpty)
				return alreadyCollectedInfos;

			foreach (ITypeSymbol constraintType in types)
			{
				_cancellation.ThrowIfCancellationRequested();

				switch (constraintType)
				{
					case ITypeParameterSymbol otherTypeParameter:
						alreadyCollectedInfos = GetBannedInfosFromTypeParameter(otherTypeParameter, alreadyCollectedInfos, checkInterfaces);
						continue;

					case INamedTypeSymbol namedType:
						alreadyCollectedInfos = GetBannedInfosFromTypeSymbolAndItsHierarchy(namedType, alreadyCollectedInfos, checkInterfaces);
						continue;
				}
			}

			return alreadyCollectedInfos;
		}

		private bool IsInWhiteList(ISymbol symbol)
		{
			if (_allowedInfoRetriever?.GetInfoForApi(symbol) is ApiSearchResult)
			{
				if (symbol.ContainingNamespace != null && !symbol.ContainingNamespace.IsGlobalNamespace)
					NamespacesWithUsedWhiteListedMembers.Add(symbol.ContainingNamespace.ToString());

				return true;
			}

			return false;
		}
	}
}
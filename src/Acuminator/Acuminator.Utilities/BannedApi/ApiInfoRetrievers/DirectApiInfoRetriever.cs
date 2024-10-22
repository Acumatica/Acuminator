using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Model;
using Acuminator.Utilities.BannedApi.Storage;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.BannedApi.ApiInfoRetrievers;

/// <summary>
/// A retriever of the API info that only searches for the information of the API itself, all containing APIs are not checked.
/// </summary>
public class DirectApiInfoRetriever(IApiStorage apiStorage, CodeAnalysisSettings codeAnalysisSettings) : IApiInfoRetriever
{
	protected CodeAnalysisSettings AnalysisSettings { get; } = codeAnalysisSettings.CheckIfNull();

	protected IApiStorage Storage { get; } = apiStorage.CheckIfNull();

	public ApiSearchResult? GetInfoForApi(ISymbol apiSymbol)
	{
		ApiKind apiKind = apiSymbol.GetApiKind();
		ApiSearchResult? directApiInfo = GetInfoForApiImpl(apiSymbol, apiKind);

		return directApiInfo;
	}

	protected virtual ApiSearchResult? GetInfoForApiImpl(ISymbol apiSymbol, ApiKind apiKind)
	{
		var (apiSymbolFoundInDb, symbolName) = GetInfoForSymbol(apiSymbol, apiKind);
		return apiSymbolFoundInDb != null
			? new ApiSearchResult(closestBannedApi: apiSymbolFoundInDb, apiSymbolFoundInDb, 
								  closestBannedApiSymbolName: symbolName!, apiFoundInDbSymbolName: symbolName!)
			: null;
	}

	protected (Api? Info, string? SymbolName) GetInfoForSymbol(ISymbol symbol, ApiKind symbolKind) =>
		symbolKind switch
		{
			ApiKind.Method => GetInfoForMethodSymbol(symbol as IMethodSymbol),
			ApiKind.Type   => GetInfoForTypeSymbol(symbol as INamedTypeSymbol),
			_ 			   => GetInfoForRegularSymbol(symbol, symbolKind)
		};

	protected (Api? Info, string? SymbolName) GetInfoForMethodSymbol(IMethodSymbol? method)
	{
		if (method == null)
			return default;

		if (method.MethodKind == MethodKind.ReducedExtension && method.ReducedFrom != null)
		{
			var (apiInfoForOriginalExtensionMethod, symbolName) = GetInfoForRegularSymbol(method.ReducedFrom, ApiKind.Method);

			if (apiInfoForOriginalExtensionMethod != null)
				return (apiInfoForOriginalExtensionMethod, symbolName);
		}

		if (method.OriginalDefinition != null && !SymbolEqualityComparer.Default.Equals(method, method.OriginalDefinition))
		{
			var (apiInfoForOriginalGenericMethod, symbolName) = GetInfoForRegularSymbol(method.OriginalDefinition, ApiKind.Method);

			if (apiInfoForOriginalGenericMethod != null)
				return (apiInfoForOriginalGenericMethod, symbolName);
		}

		return GetInfoForRegularSymbol(method, ApiKind.Method);
	}

	protected (Api? Info, string? SymbolName) GetInfoForTypeSymbol(INamedTypeSymbol? type)
	{
		if (type == null)
			return default;

		if (type.IsGenericType && type.OriginalDefinition != null)
		{
			var (apiInfoForOriginalGenericType, symbolName) = GetInfoForRegularSymbol(type.OriginalDefinition, ApiKind.Type);

			if (apiInfoForOriginalGenericType != null)
				return (apiInfoForOriginalGenericType, symbolName);
		}

		return GetInfoForRegularSymbol(type, ApiKind.Type);
	}

	protected (Api? Info, string? SymbolName) GetInfoForRegularSymbol(ISymbol symbol, ApiKind symbolKind)
	{
		string? symbolDocID = symbol.GetDocumentationCommentId().NullIfWhiteSpace();

		if (symbolDocID == null)
			return default;

		Api? symbolApiInfo = Storage.GetApi(symbolKind, symbolDocID);
		return symbolApiInfo?.BanKind switch
		{
			ApiBanKind.General => (symbolApiInfo, symbol.ToString()),
			ApiBanKind.ISV 	   => AnalysisSettings.IsvSpecificAnalyzersEnabled ? (symbolApiInfo, symbol.ToString()) : default,
			_ 				   => default
		};
	}
}
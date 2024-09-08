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
		var apiSymbolFoundInDb = GetInfoForSymbol(apiSymbol, apiKind);
		return apiSymbolFoundInDb != null
			? new ApiSearchResult(closestBannedApi: apiSymbolFoundInDb, apiSymbolFoundInDb)
			: null;
	}

	protected Api? GetInfoForSymbol(ISymbol symbol, ApiKind symbolKind)
	{
		string? symbolDocID = symbol.GetDocumentationCommentId().NullIfWhiteSpace();

		if (symbolDocID == null)
			return null;

		var symbolApiInfo = Storage.GetApi(symbolKind, symbolDocID);
		return symbolApiInfo?.BanKind switch
		{
			ApiBanKind.General => symbolApiInfo,
			ApiBanKind.ISV 	   => AnalysisSettings.IsvSpecificAnalyzersEnabled ? symbolApiInfo : null,
			_ 				   => null
		};
	}
}
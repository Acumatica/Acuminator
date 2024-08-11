#nullable enable

using System;
using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Acuminator.Utilities.ForbiddenApi.ApiInfoRetrievers
{
    /// <summary>
    /// A retriever of the API info that caches the result of another retriever.
    /// </summary>
    public class ApiInfoRetrieverWithWeakCache : IApiInfoRetriever
	{
		private class CacheEntry
		{
			public ApiSearchResult? Value { get; set; }
		}

		private readonly ConditionalWeakTable<ISymbol, CacheEntry> _weakCache = new();
		private readonly IApiInfoRetriever _innerApiInfoRetriever;

		public ApiInfoRetrieverWithWeakCache(IApiInfoRetriever innerApiInfoRetriever)
        {
			_innerApiInfoRetriever = innerApiInfoRetriever.CheckIfNull();
        }

		public ApiSearchResult? GetInfoForApi(ISymbol apiSymbol)
		{
			apiSymbol.CheckIfNull();

			if (_weakCache.TryGetValue(apiSymbol, out CacheEntry cacheEntry))
				return cacheEntry.Value;

			var apiInfo	   	 = _innerApiInfoRetriever.GetInfoForApi(apiSymbol);
			cacheEntry	   	 = _weakCache.GetOrCreateValue(apiSymbol);
			cacheEntry.Value = apiInfo;

			return apiInfo;
		}
	}
}
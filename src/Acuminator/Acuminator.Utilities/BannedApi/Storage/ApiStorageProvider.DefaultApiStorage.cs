#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Model;

namespace Acuminator.Utilities.BannedApi.Storage
{
	public partial class ApiStorageProvider
	{
		/// <summary>
		/// A default API storage.
		/// </summary>
		private class DefaultApiStorage : IApiStorage
		{
			private enum StringCombineResult
			{
				StringFromExistingApi,
				StringFromAddedApi,
				NewCombinedString
			}

			private readonly IReadOnlyDictionary<ApiKind, IReadOnlyDictionary<string, Api>> _ApisByDocIdGroupedByApiKind;

			public int ApiKindsCount => _ApisByDocIdGroupedByApiKind.Count;

			public DefaultApiStorage()
			{
				_ApisByDocIdGroupedByApiKind = ImmutableDictionary<ApiKind, IReadOnlyDictionary<string, Api>>.Empty;
			}

			public DefaultApiStorage(IEnumerable<Api> apis)
			{
				_ApisByDocIdGroupedByApiKind =
					apis.GroupBy(api => api.Kind)
						.ToDictionary(keySelector: groupedApi => groupedApi.Key,
									  elementSelector: GetApisOfSameKindByDocID);
			}

			private IReadOnlyDictionary<string, Api> GetApisOfSameKindByDocID(IEnumerable<Api> apis)
			{
				Dictionary<string, Api> apisByDocID = new();

				foreach (var api in apis)
				{
					if (apisByDocID.TryGetValue(api.DocID, out Api existingApi))
					{
						var (shouldReplace, resultApi) = CombineApis(existingApi, api);

						if (shouldReplace)
							apisByDocID[api.DocID] = resultApi;
					}
					else
						apisByDocID.Add(api.DocID, api);
				}

				return apisByDocID;
			}

			public int CountOfApis(ApiKind apiKind)
			{
				var apiOfThisKind = ApiByKind(apiKind);
				return apiOfThisKind?.Count ?? 0;
			}

			public Api? GetApi(ApiKind apiKind, string apiDocId)
			{
				var apisOfThisKind = ApiByKind(apiKind);
				return apisOfThisKind?.TryGetValue(apiDocId, out var bannedApi) == true
					? bannedApi
					: null;
			}

			public bool ContainsApi(ApiKind apiKind, string apiDocId) =>
				ApiByKind(apiKind)?.ContainsKey(apiDocId) ?? false;

			private IReadOnlyDictionary<string, Api>? ApiByKind(ApiKind apiKind) =>
				_ApisByDocIdGroupedByApiKind.TryGetValue(apiKind, out var apiWithBanKind)
					? apiWithBanKind
					: null;

			private static (bool ShouldReplaceEntry, Api Result) CombineApis(Api existingApi, Api apiToAdd)
			{
				ApiBanKind combinedBanKind = existingApi.BanKind.Combine(apiToAdd.BanKind);
				var (combineResult, combinedReason) = CombineBanReasons(apiToAdd.BanReason, existingApi.BanReason);

				return combineResult switch
				{
					StringCombineResult.StringFromAddedApi 	  => existingApi.BanKind == apiToAdd.BanKind
																	? (ShouldReplaceEntry: true, apiToAdd)
																	: (ShouldReplaceEntry: true, new Api(apiToAdd, combinedBanKind, apiToAdd.BanReason)),
					StringCombineResult.NewCombinedString 	  => (ShouldReplaceEntry: true, new Api(apiToAdd, combinedBanKind, combinedReason)),
					StringCombineResult.StringFromExistingApi => existingApi.BanKind == apiToAdd.BanKind
																	? (ShouldReplaceEntry: false, existingApi)
																	: (ShouldReplaceEntry: true, new Api(existingApi, combinedBanKind, existingApi.BanReason)),
					_ 										  => (ShouldReplaceEntry: false, existingApi)
				};
			}

			private static (StringCombineResult CombineResult, string? CombinedReason) CombineBanReasons(string? apiToAddReason,
																										 string? existingApiReason)
			{
				if (string.Equals(apiToAddReason, existingApiReason, StringComparison.OrdinalIgnoreCase) || apiToAddReason.IsNullOrWhiteSpace())
					return (StringCombineResult.StringFromExistingApi, existingApiReason);
				else if (existingApiReason.IsNullOrWhiteSpace())
					return (StringCombineResult.StringFromAddedApi, apiToAddReason);

				string combinedBanReason = $"{apiToAddReason}{Environment.NewLine}{existingApiReason}";
				return (StringCombineResult.NewCombinedString, combinedBanReason);
			}
		}
	}
}
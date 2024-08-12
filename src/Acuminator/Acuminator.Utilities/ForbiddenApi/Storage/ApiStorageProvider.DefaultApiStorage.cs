#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.ForbiddenApi.Model;

namespace Acuminator.Utilities.ForbiddenApi.Storage
{
	public partial class ApiStorageProvider
	{
		/// <summary>
		/// A default API storage.
		/// </summary>
		private class DefaultApiStorage : IApiStorage
		{
			private readonly IReadOnlyDictionary<ApiKind, IReadOnlyDictionary<string, ApiWithAppliedBanKind>> _ApisByDocIdGroupedByApiKind;

			public int ApiKindsCount => _ApisByDocIdGroupedByApiKind.Count;

			public DefaultApiStorage()
			{
				_ApisByDocIdGroupedByApiKind = ImmutableDictionary<ApiKind, IReadOnlyDictionary<string, ApiWithAppliedBanKind>>.Empty;
			}

			public DefaultApiStorage(IEnumerable<Api> apis)
			{
				_ApisByDocIdGroupedByApiKind =
					apis.GroupBy(api => api.Kind)
						.ToDictionary(keySelector: groupedApi => groupedApi.Key,
									  elementSelector: GetApisOfSameKindByDocID);
			}

			private IReadOnlyDictionary<string, ApiWithAppliedBanKind> GetApisOfSameKindByDocID(IEnumerable<Api> apis)
			{
				Dictionary<string, ApiWithAppliedBanKind> apisByDocID = new();

				foreach (var api in apis)
				{
					if (apisByDocID.TryGetValue(api.DocID, out ApiWithAppliedBanKind existingApi))
						existingApi.Combine(api);
					else
						apisByDocID.Add(api.DocID, new ApiWithAppliedBanKind(api));
				}

				return apisByDocID;
			}

			public int CountOfApis(ApiKind apiKind)
			{
				var apiOfThisKind = ApiByKind(apiKind);
				return apiOfThisKind?.Count ?? 0;
			}

			public ApiWithAppliedBanKind? GetApi(ApiKind apiKind, string apiDocId)
			{
				var apisOfThisKind = ApiByKind(apiKind);
				return apisOfThisKind?.TryGetValue(apiDocId, out var bannedApi) == true
					? bannedApi
					: null;
			}

			public bool ContainsApi(ApiKind apiKind, string apiDocId) =>
				ApiByKind(apiKind)?.ContainsKey(apiDocId) ?? false;

			private IReadOnlyDictionary<string, ApiWithAppliedBanKind>? ApiByKind(ApiKind apiKind) =>
				_ApisByDocIdGroupedByApiKind.TryGetValue(apiKind, out var apiWithBanKind)
					? apiWithBanKind
					: null;
		}
	}
}
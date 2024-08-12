#nullable enable

using System;
using System.Linq;

using Acuminator.Utilities.Common;

namespace Acuminator.Utilities.BannedApi.Model
{
	public class ApiWithAppliedBanKind
	{
		private enum StringCombineResult
		{
			StringFromExistingApi,
			StringFromAddedApi,
			NewCombinedString
		}

		private readonly string _docId;

		public Api? ApiBannedGeneral { get; private set; }

		public Api? ApiBannedIsv { get; private set; }

		public ApiWithAppliedBanKind(Api firstAddedApi)
		{
			_docId = firstAddedApi.CheckIfNull().DocID;

			switch (firstAddedApi.BanKind)
			{
				case ApiBanKind.General:
					ApiBannedGeneral = firstAddedApi;
					break;
				case ApiBanKind.ISV:
					ApiBannedIsv = firstAddedApi;
					break;
				case ApiBanKind.All:
					ApiBannedGeneral = firstAddedApi;
					ApiBannedIsv = firstAddedApi;
					break;
			}
		}

		public void Combine(Api apiToAdd)
		{
			if (apiToAdd.CheckIfNull().DocID != _docId)
				throw new ArgumentException($"APIs with different {nameof(apiToAdd.DocID)}s cannot be combined.", nameof(apiToAdd));

			switch (apiToAdd.BanKind)
			{
				case ApiBanKind.General:
					ApiBannedGeneral = CombineApis(ApiBannedGeneral, apiToAdd, ApiBanKind.General);
					return;
				case ApiBanKind.ISV:
					ApiBannedIsv = CombineApis(ApiBannedIsv, apiToAdd, ApiBanKind.ISV);
					return;
				case ApiBanKind.All:
					ApiBannedGeneral = CombineApis(ApiBannedGeneral, apiToAdd, ApiBanKind.General);
					ApiBannedIsv	 = CombineApis(ApiBannedIsv, apiToAdd, ApiBanKind.ISV);
					return;
				case ApiBanKind.None:
				default:
					return;
			}
		}

		private static Api CombineApis(Api? existingApi, Api apiToAdd, ApiBanKind resultBanKind)
		{
			if (existingApi == null)
			{
				return apiToAdd.BanKind == resultBanKind
					? apiToAdd
					: new Api(apiToAdd, resultBanKind, apiToAdd.BanReason);
			}

			var (combineResult, combinedReason) = CombineBanReasons(apiToAdd.BanReason, existingApi.BanReason);
			return combineResult switch
			{
				StringCombineResult.StringFromAddedApi => apiToAdd.BanKind == resultBanKind
															? apiToAdd
															: new Api(apiToAdd, resultBanKind, apiToAdd.BanReason),
				StringCombineResult.NewCombinedString  => new Api(existingApi, resultBanKind, combinedReason),
				_ 									   => existingApi
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

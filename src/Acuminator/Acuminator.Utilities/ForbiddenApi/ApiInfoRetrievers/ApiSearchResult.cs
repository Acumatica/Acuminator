#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.ForbiddenApi.Model;

namespace Acuminator.Utilities.ForbiddenApi.ApiInfoRetrievers
{
	/// <summary>
	/// API search result in the banned API database.
	/// </summary>
	public readonly struct ApiSearchResult
	{
		public Api ClosestBannedApi { get; }

		public Api ApiFoundInDB { get; }

        public ApiSearchResult(Api closestBannedApi, Api apiFoundInDB)
        {
			ClosestBannedApi = closestBannedApi.CheckIfNull();
			ApiFoundInDB	 = apiFoundInDB.CheckIfNull();
        }

		public void Deconstruct(out Api closestBannedApi, out Api apiFoundInDB)
		{
			closestBannedApi = ClosestBannedApi;
			apiFoundInDB 	 = ApiFoundInDB;
		}
    }
}

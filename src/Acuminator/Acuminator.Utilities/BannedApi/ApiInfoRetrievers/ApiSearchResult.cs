#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Model;

namespace Acuminator.Utilities.BannedApi.ApiInfoRetrievers
{
	/// <summary>
	/// API search result in the banned API database.
	/// </summary>
	public readonly struct ApiSearchResult
	{
		public string ClosestBannedApiSymbolName { get; }

		public string ApiFoundInDbSymbolName { get; }

		public Api ClosestBannedApi { get; }

		public Api ApiFoundInDB { get; }

        public ApiSearchResult(Api closestBannedApi, Api apiFoundInDB, string closestBannedApiSymbolName, string apiFoundInDbSymbolName)
        {
			ClosestBannedApi 		   = closestBannedApi.CheckIfNull();
			ApiFoundInDB	 		   = apiFoundInDB.CheckIfNull();
			ClosestBannedApiSymbolName = closestBannedApiSymbolName.CheckIfNullOrWhiteSpace();
			ApiFoundInDbSymbolName 	   = apiFoundInDbSymbolName.CheckIfNullOrWhiteSpace();
        }
    }
}

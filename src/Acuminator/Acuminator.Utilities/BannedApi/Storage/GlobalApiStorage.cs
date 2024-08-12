#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.BannedApi.Providers;

namespace Acuminator.Utilities.BannedApi.Storage
{
    /// <summary>
    /// A shared storage for forbidden APIs available both to analyzers and VSIX.
    /// </summary>
    public class GlobalApiStorage
	{
		private const string _bannedApiAssemblyResourceName = @"StaticAnalysis.ForbiddenAPI.Data.BannedApis.txt";
		private const string _whiteListAssemblyResourceName = @"StaticAnalysis.ForbiddenAPI.Data.WhiteList.txt";

		public static GlobalApiStorage? VsixInstance { get; set; } 

		public static GlobalApiStorage Default { get; } = new GlobalApiStorage();


	}
}

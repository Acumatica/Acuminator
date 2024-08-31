#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities.BannedApi.Storage
{
    /// <summary>
    /// A shared storage for forbidden APIs available both to analyzers and VSIX.
    /// </summary>
    public class GlobalApiStorage
	{
		public static GlobalApiStorage? VsixInstance { get; set; } 

		public static GlobalApiStorage Default { get; } = new GlobalApiStorage();
	}
}

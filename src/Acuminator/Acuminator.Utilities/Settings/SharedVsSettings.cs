using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Utilities
{
	public static class SharedVsSettings
	{
		public static VSVersion VSVersion { get; set; }

		public static bool IsOutOfProcessAnalysis => VSVersion == null; 
	}
}

using System;
using System.Collections;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph.Sources
{
	public class POCustomOrderEntry : PXGraph<POCustomOrderEntry>
    {
		public static int Property { get; set; }
	}
}

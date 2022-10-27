using System;
using System.Collections;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph.Sources
{
	public class POCustomOrderEntry : PXGraph<POCustomOrderEntry>
    {
		public static readonly int Field = 1;
	}
}

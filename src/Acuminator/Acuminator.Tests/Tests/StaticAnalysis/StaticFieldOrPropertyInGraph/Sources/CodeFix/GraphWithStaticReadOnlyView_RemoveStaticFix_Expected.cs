using System;
using System.Collections;
using PX.Data;
using PX.Objects.PO;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph.Sources
{
	public class POCustomOrderEntry : PXGraph<POCustomOrderEntry>
    {
		public readonly PXSelect<POOrder,
								  Where<POOrder.orderNbr, Equal<Current<POOrder.orderNbr>>,
									And<POOrder.orderType, Equal<Current<POOrder.orderType>>>>> CurrentOrder;
	}
}

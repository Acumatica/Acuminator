﻿using System;
using System.Collections;
using PX.Data;
using PX.Objects.PO;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph.Sources
{
	public class POCustomOrderEntry : PXGraph<POCustomOrderEntry, POOrder>
    {
		public static int Field = 1;

		public static int Property { get; set; } = 1;

		public static readonly int ReadonlyField = 1;

		public static int ReadonlyProperty { get; } = 1;

		public static PXSelect<POOrder> CustomOrders;

		public static readonly PXSelect<POOrder,
								  Where<POOrder.orderNbr, Equal<Current<POOrder.orderNbr>>,
									And<POOrder.orderType, Equal<Current<POOrder.orderType>>>>> CurrentOrder;

		public static readonly PXAction<POOrder> ReleaseOrder;

		[PXButton]
		[PXUIField]
		public virtual void releaseOrder()
		{
			
		}


		public const int Constant = 5;			// Should not be reported
	}
}

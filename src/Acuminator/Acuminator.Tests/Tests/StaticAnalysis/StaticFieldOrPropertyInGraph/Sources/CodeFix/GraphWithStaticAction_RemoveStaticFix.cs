using System;
using System.Collections;
using PX.Data;
using PX.Objects.PO;

namespace Acuminator.Tests.Tests.StaticAnalysis.StaticFieldOrPropertyInGraph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class POCustomOrderEntryExt : PXGraphExtension<POCustomOrderEntry>
    {
		public static PXAction<POOrder> ReleaseOrder;

		[PXButton]
		[PXUIField]
		public virtual void releaseOrder()
		{

		}
	}

	public class POCustomOrderEntry : PXGraph<POCustomOrderEntry>
	{

	}
}

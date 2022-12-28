using PX.Data;
using PX.Objects.AR;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseActionHandler.Sources
{	
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public abstract class PaymentTransactionGraphtExtBase<TGraph, TPrimary> : PXGraphExtension<TGraph>
	where TGraph : PXGraph, new()
	where TPrimary : class, IBqlTable, new()
	{
		public PXAction<ARPayment> captureOnlyCCPayment;

		[PXUIField(DisplayName = "Record and Capture Preauthorization", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public virtual IEnumerable CaptureOnlyCCPayment(PXAdapter adapter)
		{
			yield break;
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class ARPaymentEntryPaymentTransactionExt : PaymentTransactionGraphtExtBase<ARPaymentEntry, ARPayment>
	{
		[PXUIField(DisplayName = "Record and Capture Preauthorization", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable CaptureOnlyCCPayment(PXAdapter adapter)
		{
			if (adapter == null)
			{
				return base.CaptureOnlyCCPayment(adapter);
			}

			return adapter.Get();
		}
	}
}
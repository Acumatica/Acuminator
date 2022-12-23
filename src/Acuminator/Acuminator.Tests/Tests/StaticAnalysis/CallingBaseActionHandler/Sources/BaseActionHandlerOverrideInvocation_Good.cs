using PX.Data;
using PX.SM;
using System;
using System.Collections;

namespace Acuminator.Tests.Tests.StaticAnalysis.CallingBaseActionHandler.Sources
{
	public class UserMaint : PXGraph<UserMaint, Users>
	{
		public PXSelect<Users> AllUsers;

		public PXAction<Users> SyncUsers;

		[PXButton]
		[PXUIField(DisplayName = "Sync Users")]
		public virtual IEnumerable syncUsers(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UserMaintExtBase : PXGraphExtension<UserMaint>
	{
		public PXAction<Users> SyncUsers;

		[PXButton]
		[PXUIField(DisplayName = "Sync Users")]
		public virtual IEnumerable syncUsers(PXAdapter adapter)
		{
			return Base.syncUsers(adapter);
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class UserMaintExt : UserMaintExtBase
	{
		[PXUIField(DisplayName = "Record and Capture Preauthorization", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
		[PXProcessButton]
		public override IEnumerable CaptureOnlyCCPayment(PXAdapter adapter)
		{
			if (this.Base.Document.Current != null &&
					this.Base.Document.Current.Released == false &&
					this.Base.Document.Current.IsCCPayment == true
					&& ccPaymentInfo.AskExt(initAuthCCInfo) == WebDialogResult.OK)
			{
				return base.CaptureOnlyCCPayment(adapter);
			}
			ccPaymentInfo.View.Clear();
			ccPaymentInfo.Cache.Clear();
			return adapter.Get();
		}
	}
}
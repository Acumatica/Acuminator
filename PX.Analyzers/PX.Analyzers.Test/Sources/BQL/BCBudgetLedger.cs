using System;
using PX.Data;

namespace PX.Objects.SO
{
	[Serializable]
	public class BCBudgetLedger : IBqlTable
	{
		#region LedgerID
		public abstract class ledgerID : IBqlField
		{ }

		[PXDBString(32, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Ledger")]
		public virtual string LedgerID
		{
			get;
			set;
		}

		#endregion

		#region Description
		public abstract class description : IBqlField
		{ }
		
		[PXDBString(255, IsUnicode = true)]
		[PXUIField(DisplayName = "Description")]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion

		#region Summarize
		public abstract class summarize : IBqlField
		{
		}
		
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Summarize")]
		public virtual bool? Summarize
		{
			get;
			set;
		}
		#endregion

	}
}

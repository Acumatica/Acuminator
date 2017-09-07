using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.SO
{
	[Serializable]
	public class BCBudgetBalance : IBqlTable
	{
		#region LedgerID
		public abstract class ledgerID : IBqlField
		{
		}

		[PXDBString(32, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Ledger")]
		public virtual string LedgerID
		{
			get;
			set;
		}
		#endregion

		#region SubledgerID
		public abstract class subledgerID : IBqlField
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? SubledgerID
		{
			get;
			set;
		}
		#endregion

		#region BliID
		public abstract class bliID : IBqlField
		{
		}

		[PXDBInt(IsKey = true)]
		[PXDefault]
		public virtual int? BliID
		{
			get;
			set;
		}
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : IBqlField
		{
		}

		[PXDBString(6, IsFixed = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Fin. Period")]
		public virtual string FinPeriodID
		{
			get;
			set;
		}
		#endregion

		#region Amount
		public abstract class amount : IBqlField
		{
		}

		[PXDBDecimal(19)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? Amount
		{
			get;
			set;
		}
		#endregion

		#region YtdAmount
		public abstract class ytdAmount : IBqlField
		{
		}

		[PXDBDecimal(19)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "YTD Amount")]
		public virtual decimal? YtdAmount
		{
			get;
			set;
		}
		#endregion
	}
}

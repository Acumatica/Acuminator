using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;
using PX.Objects.GL;

namespace PX.Objects.HackathonDemo.Extensions.NonPublic
{
	[PXHidden]
	public class SOOrder : IBqlTable
	{
        #region OrderType
        public abstract class orderType : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion

		#region OrderNbr
		public abstract class orderNbr : IBqlField { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Nbr.")]
		public string OrderNbr { get; set; }
		#endregion

		#region FinPeriodID
		public abstract class finPeriodID : IBqlField { }

		[FinPeriodSelector]
		[PXDefault]
		[PXUIField(DisplayName = "Fin. Period")]
		public string FinPeriodID { get; set; }
		#endregion

		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBLocalizableString(255, IsUnicode = true, NonDB = true, BqlField = typeof(CA.PaymentMethod.descr))]
		[PXDefault("", PersistingCheck = PXPersistingCheck.NullOrBlank)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		[PXDBCalced(
			typeof(Switch<Case<Where<CA.PaymentMethod.descr, IsNotNull>, CA.PaymentMethod.descr>, CustomerPaymentMethod.descr>),
			typeof(string))]
		public virtual string Descr { get; set; }

		#endregion

		#region Status
		public abstract class status : IBqlField { }

		[PXStringList(new[] { "N", "O" }, new[] { "New", "Open" })]
		[PXUIField(DisplayName = "Status")]
		[PXString]
		public string Status { get; set; }
		#endregion

        #region tstamp
        public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class APInvoice : IBqlTable
	{
		#region BranchID
		public class branchID : IBqlField
		{
		}

		[PXDBInt]
		[APBranch]                  //Here the APBranch Attribute has PXDBString and PXDBInt on aggregators which is not correct. Moreover, there is another PXDBInt type attributes on the property
		public virtual int? BranchID { get; set; }
		#endregion

		#region RefNbr
		public abstract class refNbr : IBqlField
		{
		}

		/// <summary>
		/// [key] Reference number of the document.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Reference Nbr.", Visibility = PXUIVisibility.SelectorVisible, TabOrder = 1)]
		public virtual String RefNbr { get; set; }
		#endregion

		#region DocType
		public abstract class docType : IBqlField
		{
		}

		[PXDBString(3, IsKey = true, IsFixed = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Type", Visibility = PXUIVisibility.SelectorVisible, Enabled = true, TabOrder = 0)]
		public virtual String DocType { get; set; }
		#endregion

		#region Released
		public abstract class released : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Released", Visible = false)]
		public virtual bool? Released { get; set; }
		#endregion

		#region Hold
		public abstract class hold : IBqlField
		{
		}

		[PXDBBool]
		[PXUIField(DisplayName = "Hold", Visibility = PXUIVisibility.Visible)]
		[PXDefault(true)]
		public virtual bool? Hold { get; set; }
		#endregion

		#region DocDate
		public abstract class docDate : IBqlField
		{
		}

		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Date", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual DateTime? DocDate { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField
		{
		}

		[PXDBLong]
		[CurrencyInfo]
		public virtual long? CuryInfoID { get; set; }
		#endregion

		#region CuryDiscTot
		public abstract class curyDiscTot : IBqlField
		{
		}

		[PXDBDecimal]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Discount Total", Enabled = true)]
		public virtual decimal? CuryDiscTot { get; set; }
		#endregion

		#region CuryDocBal
		public abstract class curyDocBal : IBqlField
		{
		}
		
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.docBal))]
		[PXUIField(DisplayName = "Balance", Visibility = PXUIVisibility.SelectorVisible, Enabled = false)]
		public virtual decimal? CuryDocBal { get; set; }
		#endregion

		#region DocBal
		public abstract class docBal : IBqlField
		{
		}

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public virtual decimal? DocBal { get; set; }
		#endregion

		#region CuryOrigDocAmt
		public abstract class curyOrigDocAmt : IBqlField
		{
		}

		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXDBCurrency(typeof(APInvoice.curyInfoID), typeof(APInvoice.origDocAmt))]
		[PXUIField(DisplayName = "Amount", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual decimal? CuryOrigDocAmt { get; set; }
		#endregion

		#region OrigDocAmt
		public abstract class origDocAmt : IBqlField
		{
		}

		[PXDBBaseCury]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? OrigDocAmt { get; set; }
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

using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;

namespace PX.Objects
{
	/// <summary>
	/// Some description here to avoid displaying diagnostic on the DAC itself.
	/// </summary>
	[PXProjection(typeof(Select2<GLTran,
			LeftJoin<Branch, On<Branch.branchID, Equal<GLTran.branchID>>>,
			Where<GLTran.released, Equal<True>>>))]
	[PXCacheName("Projection DAC")]
	public class GLTranScoreWithReclass : GLTran
	{
		#region TranID
		public new abstract class tranID : BqlInt.Field<tranID> { }

		[PXDBInt(IsKey = true)]
		public override int? TranID { get; set; }
		#endregion

		#region BatchNbr
		public new abstract class batchNbr : BqlString.Field<batchNbr> { }

		/// <value>
		/// The batch number.
		/// </value>
		[PXDBString(15, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public override string BatchNbr { get; set; }
		#endregion

		#region IsInterCompany
		public new abstract class isInterCompany : BqlBool.Field<isInterCompany> { }

		[PXDBBool]
		[PXDefault(false)]
		public new virtual bool? IsInterCompany { get; set; }
		#endregion

		#region LineNbr
		public new abstract class lineNbr : BqlInt.Field<lineNbr> { }

		/// <summary>
		/// 
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public override int? LineNbr { get; set; }
		#endregion

		#region BAccount
		public abstract class bAccountID : BqlInt.Field<bAccountID> { }

		/// <remarks>
		/// The remark
		/// </remarks>
		[PXDBInt(BqlField = typeof(Branch.bAccountID))]
		[PXUIField(DisplayName = "BAccount", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual int? BAccount { get; set; }
		#endregion
	}
}

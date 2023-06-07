using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.BQL;
using PX.Objects.GL;

namespace PX.Objects
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	/// <summary>
	/// Some description here to avoid displaying diagnostic
	/// </summary>
	public sealed class GLTranScoreWithReclassExt : PXCacheExtension<GLTranScoreWithReclass>
	{
		#region TranID
		public abstract class tranID : BqlInt.Field<tranID> { }

		/// <inheritdoc cref="GLTran.TranID"/>
		[PXDBInt(IsKey = true, BqlField = typeof(GLTran.tranID))]
		public int? TranID { get; set; }
		#endregion

		#region BatchNbr
		public abstract class batchNbr : BqlString.Field<batchNbr> { }

		/// <inheritdoc cref="GLTran.BatchNbr"/>
		/// <value>
		/// The batch number.
		/// </value>
		[PXDBString(15, IsUnicode = true, BqlTable = typeof(GLTran))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public string BatchNbr { get; set; }
		#endregion

		#region LineNbr
		public abstract class lineNbr : BqlInt.Field<lineNbr> { }

		/// <inheritdoc cref="GLTran.LineNbr"/>
		[PXDBInt(BqlTable = typeof(GLTran))]
		[PXUIField(DisplayName = "Line Nbr.", Visibility = PXUIVisibility.Visible, Visible = false, Enabled = false)]
		public int? LineNbr { get; set; }
		#endregion

		#region TranDate
		public abstract class tranDate : BqlDateTime.Field<tranDate> { }

		/// <inheritdoc cref="GLTran.TranDate"/>
		/// <remarks>
		/// The remark
		/// </remarks>
		[PXDBDate(BqlField = typeof(GLTran.tranDate))]
		[PXUIField(DisplayName = "Transaction Date", Visibility = PXUIVisibility.Visible, Enabled = false)]
		public DateTime? TranDate { get; set; }
		#endregion
	}

	/// <summary>
	/// Some description here to avoid displaying diagnostic on the DAC itself.
	/// </summary>
	[PXProjection(typeof(Select2<GLTran,
			LeftJoin<Branch, On<Branch.branchID, Equal<GLTran.branchID>>>,
			Where<GLTran.released, Equal<True>>>))]
	[PXCacheName("Projection DAC")]
	public class GLTranScoreWithReclass : IBqlTable
	{
		
	}
}

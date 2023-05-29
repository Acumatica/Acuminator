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
	public class GLTranScoreWithReclass : IBqlTable
	{
		#region TranID
		public abstract class tranID : BqlInt.Field<tranID> { }

		/// <inheritdoc cref="GLTran.TranID"/>
		[PXDBInt(IsKey = true, BqlField = typeof(GLTran.tranID))]
		public virtual int? TranID { get; set; }
		#endregion

		#region Module
		public abstract class module : BqlString.Field<module> { }

		/// <inheritdoc cref="GLTran.Module"/>
		[PXDBString(2, IsKey = true, BqlField = typeof(GLTran.module))]
		public virtual string Module { get; set; }
		#endregion

		#region BatchNbr
		public abstract class batchNbr : BqlString.Field<batchNbr> { }

		/// <inheritdoc cref="GLTran.BatchNbr"/>
		/// <value>
		/// The batch number.
		/// </value>
		[PXDBString(15, IsUnicode = true, BqlTable = typeof(GLTran))]
		[PXUIField(DisplayName = "Batch Number", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string BatchNbr { get; set; }
		#endregion
	}
}

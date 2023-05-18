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
		#region FinPeriodID
		public abstract class finPeriodID : BqlString.Field<finPeriodID> { }

		/// <summary/>
		/// <inheritdoc/>
		/// <remarks>
		/// Error because inheritdoc on non-overridden property should point to the mapped property
		/// </remarks>
		[FinPeriodID(
			branchSourceType: typeof(GLTran.branchID),
			masterFinPeriodIDType: typeof(GLTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(Batch.tranPeriodID)
			, BqlField = typeof(GLTran.finPeriodID))]
		[PXUIField(DisplayName = "Period ID", Enabled = false, Visible = false)]
		public virtual string FinPeriodID { get; set; }
		#endregion
	}

	/// <summary>
	/// Some description here to avoid displaying diagnostic on the DAC itself.
	/// </summary>
	[PXCacheName("Derived Projection DAC")]
	public class DerivedGLTranScoreWithReclass : GLTranScoreWithReclass
	{
		/// <inheritdoc/>
		/// <remarks>
		/// Empty inheritdoc should be ok on property override
		/// </remarks>
		[FinPeriodID(
			branchSourceType: typeof(GLTran.branchID),
			masterFinPeriodIDType: typeof(GLTran.tranPeriodID),
			headerMasterFinPeriodIDType: typeof(Batch.tranPeriodID)
			, BqlField = typeof(GLTran.finPeriodID))]
		[PXUIField(DisplayName = "Derived Period ID", Enabled = false, Visible = false)]
		public override string FinPeriodID 
		{ 
			get => base.FinPeriodID; 
			set => base.FinPeriodID = value; 
		}
	}
}

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
		// Unbound not mapped field
		#region ManualEdit
		public abstract class manualEdit : BqlBool.Field<manualEdit> { }

		[PXBool]
		[PXUIField(DisplayName = "Manual Edit")]
		public virtual bool? ManualEdit
		{
			get;
			set;
		}
		#endregion

		#region TranID
		public abstract class tranID : BqlInt.Field<tranID> { }

		/// <inheritdoc cref="GLTran.tranID"/>
		/// <remarks>
		/// Described field.
		/// </remarks>
		[PXDBInt(IsKey = true, BqlField = typeof(GLTran.tranID))]
		public virtual int? TranID { get; set; }
		#endregion

		#region Module
		public abstract class module : BqlString.Field<module> { }
		
		/// <exclude/>
		/// <remarks>
		/// Excluded field.
		/// </remarks>
		[PXDBString(2, IsFixed = true, BqlField = typeof(GLTran.module))]
		[PXUIField(DisplayName = "Module", Visibility = PXUIVisibility.Visible, Visible = false)]
		public virtual string Module { get; set; }
		#endregion
	}
}

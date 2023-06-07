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
		//System field can be not documented
		#region tstamp
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }

		[PXDBTimestamp(BqlField = typeof(GLTran.Tstamp))]
		public virtual Byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

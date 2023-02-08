using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.GL;

namespace PX.Objects.HackathonDemo
{
	[Serializable]
	[PXNonInstantiatedExtension]
	public sealed class BCBindingCommerce : IBqlTable
	{
		#region BranchID
		[Branch(typeof(AccessInfo.branchID))]
		[PXDefault(typeof(AccessInfo.branchID))]
		public int? BranchID { get; set; }

		public abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }
		#endregion
	}
}
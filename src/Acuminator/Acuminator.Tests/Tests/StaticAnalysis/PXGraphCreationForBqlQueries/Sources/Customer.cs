using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class Customer : IBqlTable
	{
		#region BAccountID
		[PXDBIdentity]
		public int? BAccountID { get; set; }
		public abstract class bAccountID : IBqlField { }
		#endregion

		#region AcctCD
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string AcctCD { get; set; }
		public abstract class acctCD : IBqlField { }
		#endregion	
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOOrderExt : PXCacheExtension<SOOrder>
	{
		#region BAccountID
		[PXDBInt]
		[PXSelector(typeof(Search<
			BAccount.bAccountID, 
			Where<BAccount.bAccountID, Equal<Current<SOOrder.bAccountID>>>>))]
		public int? BAccountID { get; set; }
		public abstract class bAccountID : IBqlField { }
		#endregion
	}
}

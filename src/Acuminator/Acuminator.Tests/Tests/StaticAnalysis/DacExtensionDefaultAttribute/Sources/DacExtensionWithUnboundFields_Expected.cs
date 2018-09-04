using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderWithTotal : PXCacheExtension<SOOrder>
	{
		#region TotalAmount
		public abstract class totalAmount : IBqlField { }

		[PXDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal TotalAmount { get; set; }
		#endregion

		#region UsrNPCADocLineCntr
		public abstract class usrNPCADocLineCntr : IBqlField { }
		[PXInt]
		[PXUnboundDefault]
		public virtual int? UsrNPCADocLineCntr { get; set; }
		#endregion

		#region UsrNPCADocLineCntr2
		public abstract class usrNPCADocLineCntr2 : IBqlField { }
		[PXInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr2 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr3
		public abstract class usrNPCADocLineCntr3 : IBqlField { }
		[PXInt]
		[PXUnboundDefault]
		public virtual int? UsrNPCADocLineCntr3 { get; set; }
		#endregion
	}
}
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

		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal TotalAmount { get; set; }
		#endregion

		#region UsrNPCADocLineCntr
		public abstract class usrNPCADocLineCntr : IBqlField { }
		[PXDBInt]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr { get; set; }
		#endregion

		#region UsrNPCADocLineCntr2
		public abstract class usrNPCADocLineCntr2 : IBqlField { }
		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr2 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr3
		public abstract class usrNPCADocLineCntr3 : IBqlField { }
		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr3 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr4
		public abstract class usrNPCADocLineCntr4 : IBqlField { }
		[PXDBInt]
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr4 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr5
		public abstract class usrNPCADocLineCntr5 : IBqlField { }
		[PXDBInt, PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr5 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr6
		public abstract class usrNPCADocLineCntr6 : IBqlField { }
		[PXDBInt, PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? UsrNPCADocLineCntr6 { get; set; }
		#endregion

		#region UsrNPCADocLineCntr7
		public abstract class usrNPCADocLineCntr7 : IBqlField { }
		[PXDefault(0, PersistingCheck = PXPersistingCheck.Nothing), PXDBInt]
		public virtual int? UsrNPCADocLineCntr7 { get; set; }
		#endregion
	}

    public class SOOrder : IBqlTable
    {
    }
}
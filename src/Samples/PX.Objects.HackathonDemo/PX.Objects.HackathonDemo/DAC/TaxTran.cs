using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class TaxTran : IBqlTable
	{
		#region TaxPeriodID
		public abstract class taxPeriodID : IBqlField
		{
		}
		
		[FinPeriodID]
		public virtual string TaxPeriodID { get; set; }
		#endregion

		#region Released
		public abstract class released : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Released { get; set; }
		#endregion

		#region Voided
		public abstract class voided : IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? Voided { get; set; }
		#endregion

		#region VendorID
		public abstract class vendorID : IBqlField
		{
		}

		[PXDBInt]
		public virtual int? VendorID { get; set; }
		#endregion

		#region RevisionID
		public abstract class revisionID : IBqlField
		{
		}

		[PXDBInt]
		public virtual int? RevisionID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

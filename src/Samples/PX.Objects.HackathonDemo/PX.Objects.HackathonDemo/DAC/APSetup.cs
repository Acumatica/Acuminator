using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class APSetup : IBqlTable
	{
		#region RequireControlTotal
		public abstract class requireControlTotal : PX.Data.IBqlField
		{
		}

		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Validate Document Totals on Entry")]
		public virtual bool? RequireControlTotal { get; set; }
		#endregion

		#region RequireControlTaxTotal
		public abstract class requireControlTaxTotal : IBqlField { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Validate Tax Totals on Entry")]
		public virtual bool? RequireControlTaxTotal { get; set; }
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo.Suppression.Comments
{
	public partial class POOrder : IBqlTable
	{
		#region CompanyMask
		public abstract class companyMask : IBqlField { }
		[PXDefault]
		[PXUIField(DisplayName = "Company Mask")]
		public string CompanyMask { get; set; }
		#endregion

		#region Total
		public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}
}
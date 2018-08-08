using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public partial class SOOrder : IBqlTable
	{
		#region OrderCD
		public abstract class orderCD : IBqlField { }
		[PXDefault]
		[PXUIField(DisplayName = "Order CD")]
		public int? OrderCD { get; set; }
		#endregion
		#region CompanyMask
		public abstract class companyMask : IBqlField { }
		[PXDefault]
		[PXUIField(DisplayName = "Company Mask")]
		public string CompanyMask { get; set; }
		#endregion
	}
}

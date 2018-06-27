using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class DacExampleFieldAttributesTypeMismatch : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXString]
	    [PXDBScalar]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
	}
}

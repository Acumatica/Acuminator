using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class DacWithMultipleSpecialTypeAttributes : IBqlTable
	{
		#region OrderType
		public abstract class orderType : IBqlField { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDBScalar(typeof(Search<DacWithMultipleSpecialTypeAttributes.orderType>))]
		public string OrderType { get; set; }
		#endregion
	}
}
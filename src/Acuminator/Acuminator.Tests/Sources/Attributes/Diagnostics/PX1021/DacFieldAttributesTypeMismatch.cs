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

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		[PXUIField(DisplayName = "Order Type")]
		public string OrderType { get; set; }
		#endregion
		
		#region OrderDate
		public abstract class orderDate : IBqlField { }

		[PXDBDate]
		[PXUIField(DisplayName = "OrderDate")]
		public string OrderDate { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual bool[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

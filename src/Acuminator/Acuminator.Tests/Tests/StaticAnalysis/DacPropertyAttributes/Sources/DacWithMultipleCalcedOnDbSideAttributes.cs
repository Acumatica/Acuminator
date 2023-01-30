﻿using System;
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

		[PXString(IsKey = true, InputMask = "")]
		[PXDBScalar(typeof(Search<DacWithMultipleSpecialTypeAttributes.orderType>))]
		[PXDBCalced(typeof(Switch<Case<Where<orderType, Equal<CurrentValue<orderType>>>, True>, False>), typeof(bool))]
		public string OrderType { get; set; }
		#endregion
	}
}
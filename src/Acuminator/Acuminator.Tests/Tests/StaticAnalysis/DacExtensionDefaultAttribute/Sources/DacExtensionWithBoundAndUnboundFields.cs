﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class IIGPOALCLandedCost : PXCacheExtension<SOOrder>
    {
		#region Selected
		public abstract class selected : PX.Data.IBqlField { }
		protected bool? _Selected;
		[PXBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion

		#region Cost
		public abstract class cost : PX.Data.IBqlField { }
		protected decimal? _Cost;
		[PXDBDecimal]
		[PXDefault]
		[PXUIField(DisplayName = "Cost")]
		public virtual decimal? Cost { get; set; }
		#endregion

		#region UnboundField1
		public abstract class selected : PX.Data.IBqlField { }
		protected bool? _UnboundField1;
		[PXBool]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Unbound Field 1")]
		public virtual bool? UnboundField
		{
			get
			{
				return _UnboundField1;
			}
			set
			{
				_UnboundField1 = value;
			}
		}
		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestFbqlEntry : PXGraph<SOOrderTestFbqlEntry>
	{
		PXSelect<SOOrder,
		   Where<SOOrder.orderType, Equal<@P.AsString>,
			 And<SOOrder.orderNbr, Equal<SOOrder.orderNbr.FromCurrent>,
			 And<SOOrder.status, Equal<SOOrder.status.AsOptional>>>>,
		 OrderBy<
			 Asc<SOOrder.orderNbr>>> fieldSelect;

		public SOOrder FieldInstanceCall_Correct()
		{
			var resultWithoutOptional = fieldSelect.SelectSingle("INV");
			var resultWithOptional = fieldSelect.SelectSingle("INV", "Hold");

			return resultWithOptional ?? resultWithOptional;
		}

		public SOOrder FieldInstanceCall_Incorrect()
		{
			var result1 = fieldSelect.SelectSingle();
			var result2 = fieldSelect.SelectSingle("INV", "0000001", "Hold");

			return result1 ?? result2;
		}

		public SOOrder StaticFbqlCall_Correct()
		{
			object[] currents = null;
			SOOrder resultWithoutOptional =
				PXSelect<SOOrder,
				   Where<SOOrder.orderType, Equal<@P.AsString>,
					 And<SOOrder.orderNbr, Equal<SOOrder.orderNbr.FromCurrent>,
					 And<SOOrder.status, Equal<SOOrder.status.AsOptional>>>>,
				 OrderBy<
					 Asc<SOOrder.orderNbr>>>
				.SelectSingleBound(this, currents, "INV");
	
			var resultWithOptional =
				PXSelect<SOOrder,
				   Where<SOOrder.orderType, Equal<@P.AsString>,
					 And<SOOrder.orderNbr, Equal<SOOrder.orderNbr.FromCurrent>,
					 And<SOOrder.status, Equal<SOOrder.status.AsOptional>>>>,
				 OrderBy<
					 Asc<SOOrder.orderNbr>>>
				.SelectSingleBound(this, currents, "INV", "Hold");

			return resultWithOptional ?? resultWithoutOptional;
		}

		public SOOrder StaticFbqlCall_Incorrect()
		{
			object[] currents = null;
			SOOrder result1 =
				PXSelect<SOOrder,
				   Where<SOOrder.orderType, Equal<@P.AsString>,
					 And<SOOrder.orderNbr, Equal<SOOrder.orderNbr.FromCurrent>,
					 And<SOOrder.status, Equal<SOOrder.status.AsOptional>>>>,
				 OrderBy<
					 Asc<SOOrder.orderNbr>>>
				.SelectSingleBound(this, currents);

			var result2 =
				PXSelect<SOOrder,
				   Where<SOOrder.orderType, Equal<@P.AsString>,
					 And<SOOrder.orderNbr, Equal<SOOrder.orderNbr.FromCurrent>,
					 And<SOOrder.status, Equal<SOOrder.status.AsOptional>>>>,
				 OrderBy<
					 Asc<SOOrder.orderNbr>>>
				.SelectSingleBound(this, currents, "INV", "000001", "Hold");

			return result1 ?? result2;
		}
	}
}

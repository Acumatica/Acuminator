using System;

using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;

namespace PX.Objects.HackathonDemo
{
	public class FbqlReadOnlyEntry : PXGraph<FbqlReadOnlyEntry>
	{
		public SelectFrom<SOAdjust>
				.LeftJoin<ARRegister>
					.On<SOAdjust.adjgDocType.IsEqual<ARRegister.docType>
					.And<SOAdjust.adjgRefNbr.IsEqual<ARRegister.refNbr>>>
				.Where<SOAdjust.adjdOrderType.IsEqual<@P.AsString>
					.And<SOAdjust.adjdOrderNbr.IsEqual<@P.AsString>>
					.And<ARRegister.openDoc.IsEqual<True>>>
				.View.ReadOnly OpenAdjustingPrepaymentInvoices;

		public SOAdjust FieldInstanceCall_Correct()
		{
			SOAdjust result = OpenAdjustingPrepaymentInvoices.SelectSingle("INV", "0000001");		// no diagnostic
			return result;
		}

		public SOAdjust FieldInstanceCall_Incorrect()
		{
			var result1 = OpenAdjustingPrepaymentInvoices.SelectSingle();
			var result2 = OpenAdjustingPrepaymentInvoices.SelectSingle("INV", "0000001", "Hold");

			return result1 ?? result2;
		}

		public SOOrder StaticFbqlCall_Correct()
		{
			object[] currents = null;
			SOOrder resultWithoutOptional =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View
				.SelectSingleBound(this, currents, "INV");      // no diagnostic

			SOOrder resultWithoutOptionalReadonly =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View.ReadOnly
				.SelectSingleBound(this, currents, "INV");      // no diagnostic

			var resultWithOptional =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View
				.SelectSingleBound(this, currents, "INV", "Hold");       // no diagnostic

			var resultWithOptionalReadonly =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View.ReadOnly
				.SelectSingleBound(this, currents, "INV", "Hold");       // no diagnostic

			return resultWithOptional ?? resultWithoutOptionalReadonly ?? resultWithoutOptional ?? resultWithOptionalReadonly;
		}

		public SOOrder StaticFbqlCall_Incorrect()
		{
			object[] currents = null;
			SOOrder result1 =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View
				.SelectSingleBound(this, currents);

			SOOrder result2 =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View.ReadOnly
				.SelectSingleBound(this, currents);

			var result3 =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View
				 .SelectSingleBound(this, currents, "INV", "000001", "Hold");

			var result4 =
				SelectFrom<SOOrder>
				   .Where<SOOrder.orderType.IsEqual<@P.AsString>
					 .And<SOOrder.orderNbr.IsEqual<SOOrder.orderNbr.FromCurrent>
					 .And<SOOrder.status.IsEqual<SOOrder.status.AsOptional>>>>
				 .OrderBy<SOOrder.orderNbr.Asc>
				 .View.ReadOnly
				 .SelectSingleBound(this, currents, "INV", "000001", "Hold");

			return result1 ?? result2 ?? result3 ?? result4;
		}
	}
}

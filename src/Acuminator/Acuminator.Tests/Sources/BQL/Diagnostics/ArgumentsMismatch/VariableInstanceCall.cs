using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry3 : PXGraph<SOOrderTestEntry3>
	{
		public object TestVariableCallWithOptionalParams(SOOrder order, bool flag)
		{
			var p1 = new[] { 1, 2 };
			var p2 = new[] { 1 };

			PXSelectBase<SOOrder> filtered = new
					PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Optional<SOOrder.orderNbr>>,
					  And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			var result1 = filtered.Select(p1);
			var result2 = filtered.Select(p2);
			var result3 = filtered.Select(1,2,3);    //Diagnostic here
			return result1;
		}

		public object TestComplexVariableCall(SOOrder order, bool flag)
		{
			var p1 = new[] { 1, 2 };
			var p2 = new[] { 1 };

			PXSelectBase<SOOrder> filtered = new
					PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					  And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			var result1 = filtered.Select(p1);     //Diagnostic here
			var result2 = filtered.Select(p2);


			if (flag)
                filtered = new PXSelect<SOOrder>(this);
            else
                filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			result1 = filtered.Select();  //Should not be diagnostic here due to if statement

			filtered = new
                PXSelect<SOOrder,
                    Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
                      And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

            if (order.OrderDate.HasValue && filtered.Select(1).Count > 0)    //Diagnostic here
			{
				EscapeReferenceMethod(filtered);
				filtered.Select(1);   //Should not be diagnostic here due to EscapeReferenceMethod method
			}

			filtered = new
				PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Required<SOOrder.orderNbr>>,
					  And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			filtered.WhereAnd<Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>();

			filtered.Select(1);   //Should not be diagnostic here due to WhereAnd modifying method
			return this;
		}


		public void EscapeReferenceMethod(PXSelectBase<SOOrder> query)
		{
			//Possible modifications in the method
		}

		public void TestBqlModificationsForNonAbstractVariable(bool flag)
		{
			var p1 = new[] { 1, 2 };

			PXSelectBase<SOOrder> filtered = new
					PXSelect<SOOrder,
					Where<SOOrder.orderNbr, Equal<Current<SOOrder.orderNbr>>,
					  And<SOOrder.orderDate, Greater<Required<SOOrder.orderDate>>>>>(this);

			if (flag)
				filtered.WhereAnd<Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>>>();

			var result1 = filtered.Select(p1);   //No diagnostic should be here
		}
	}
}

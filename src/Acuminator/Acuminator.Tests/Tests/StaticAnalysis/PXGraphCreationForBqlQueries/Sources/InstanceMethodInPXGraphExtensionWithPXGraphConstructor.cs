using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class CustomerMaintExt : PXGraphExtension<CustomerMaint>
	{
		public Customer GetCustomer(int acuCustomerId)
		{
			var customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
				.Select(new PXGraph(), acuCustomerId);

			return customer.FirstTableItems.FirstOrDefault();
		}
	}
}
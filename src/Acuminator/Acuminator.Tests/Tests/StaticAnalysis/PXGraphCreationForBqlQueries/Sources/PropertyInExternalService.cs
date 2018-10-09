using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class CustomerService
	{
		public Customer BaseCustomer
		{
			get
			{
				var graph = PXGraph.CreateInstance<CustomerMaint>();

				var customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(new PXGraph(), 0);

				return customer.FirstTableItems.FirstOrDefault();
			}
		}
	}
}
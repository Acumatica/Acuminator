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
		public object GetCustomer(PXGraph graph, int acuCustomerId)
		{
			var customerGraph = PXGraph.CreateInstance<CustomerMaint>();

			var customer = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
				.Select(customerGraph, acuCustomerId);

			return customer;
		}
	}
}
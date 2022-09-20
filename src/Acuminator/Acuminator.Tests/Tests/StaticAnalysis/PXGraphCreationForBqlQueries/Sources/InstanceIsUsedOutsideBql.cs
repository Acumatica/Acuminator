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
		public void UpdateCustomer(int acuCustomerId)
		{
			var existingGraph = PXGraph.CreateInstance<CustomerMaint>();
			var graph = PXGraph.CreateInstance<CustomerMaint>();

			if (acuCustomerId > 0)
			{
				graph.Customers.Current = PXSelect<Customer, Where<Customer.bAccountID, Equal<Required<Customer.bAccountID>>>>
					.Select(graph, acuCustomerId);
			}

			graph.Actions.PressSave();
		}
	}
}
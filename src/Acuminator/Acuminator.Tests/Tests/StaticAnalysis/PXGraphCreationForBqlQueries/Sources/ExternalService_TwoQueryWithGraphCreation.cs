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
		public Customer SelectCustomer()
		{
			Customer customer1 = PXSelect<Customer>.Select(new PXGraph());								// Report diagnostic
			Customer customer2 = PXSelect<Customer>.Select(PXGraph.CreateInstance<CustomerMaint>());    // Report diagnostic

			return customer1 ?? customer2;
		}
	}
}
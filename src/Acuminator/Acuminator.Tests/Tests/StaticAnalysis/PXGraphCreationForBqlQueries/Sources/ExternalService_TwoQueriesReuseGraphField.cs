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
		public PXGraph Graph { get; }

		public CustomerService(PXGraph graph)
		{
			Graph = graph;
		}

		public Customer SelectCustomer()
		{
			Customer customer1 = PXSelect<Customer>.Select(Graph);    // Do not report diagnostic
			Customer customer2 = PXSelect<Customer>.Select(Graph);    // Do not report diagnostic

			return customer1 ?? customer2;
		}
	}
}
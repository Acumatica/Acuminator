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
		public PXGraph Graph1 { get; }

		public PXGraph Graph2 { get; }

		public CustomerService(PXGraph graph)
		{
			Graph1 = graph;
			Graph2 = graph;
		}

		public Customer SelectCustomer()
		{
			Customer customer1 = PXSelect<Customer>.Select(Graph1);    // Do not report diagnostic
			Customer customer2 = PXSelect<Customer>.Select(Graph2);    // Do not report diagnostic

			return customer1 ?? customer2;
		}
	}
}
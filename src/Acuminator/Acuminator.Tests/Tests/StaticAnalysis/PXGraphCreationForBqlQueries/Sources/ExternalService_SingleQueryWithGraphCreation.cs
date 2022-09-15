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
		public Customer SelectCustomerBaseGraphConstructor()
		{
			Customer customer = PXSelect<Customer>.Select(new PXGraph());     // Do not report diagnostic
			return customer;
		}

		public Customer SelectCustomerConcreteGraphConstructor() =>
			PXSelect<Customer>.Select(new CustomerMaint());                     // Do not report diagnostic

		public Customer SelectCustomerConcreteGraphFactoryExpressionBody() =>
			PXSelect<Customer>.Select(PXGraph.CreateInstance<CustomerMaint>());  // Do not report diagnostic

		public Customer SelectCustomerConcreteGraphFactory()
		{
			return PXSelect<Customer>.Select(PXGraph.CreateInstance<CustomerMaint>());  // Do not report diagnostic
		}
	}
}
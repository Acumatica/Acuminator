using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class CustomerMaint : PXGraph<CustomerMaint, Customer>
	{
		public PXSelect<Customer> Customers;

		public Customer SelectCustomerInstance()
		{
			var customerGraph = PXGraph.CreateInstance<CustomerMaint>();
			Customer customer = PXSelect<Customer>.Select(customerGraph);   // Report diagnostic
			return customer;
		}

		public static Customer SelectCustomerStatic()
		{
			var customerGraph = PXGraph.CreateInstance<CustomerMaint>();
			Customer customer = PXSelect<Customer>.Select(customerGraph);	// Should not report diagnostic
			return customer;
		}
	}

	[PXHidden]
	public class Customer : IBqlTable
	{
		#region AcctCD
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string AcctCD { get; set; }
		public abstract class acctCd : IBqlField { }
		#endregion	
	}
}
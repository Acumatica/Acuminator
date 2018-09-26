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
	}

	public class Customer : IBqlTable
	{
		#region AcctCD
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string AcctCD { get; set; }
		public abstract class acctCd : IBqlField { }
		#endregion	
	}
}
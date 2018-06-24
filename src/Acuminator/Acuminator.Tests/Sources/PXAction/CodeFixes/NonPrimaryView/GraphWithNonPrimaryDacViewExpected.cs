using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrder : IBqlTable { }

	public class SOTran : IBqlTable { }

	public class TestDac : IBqlTable { }

	public class SOOrderEntryWithNonPrimaryDacView1 : PXGraph<SOOrderEntryWithNonPrimaryDacView1>
	{
		public PXSelect<SOOrder> Orders;

		public PXSelect<SOTran> OrderDetails;

		public PXAction<SOOrder> Release1;

		public PXAction<SOOrder> Release2 { get; }

		public PXAction<SOOrder> Release3;
	}

	public class SOOrderEntryWithNonPrimaryDacViewExtension : PXGraphExtension<SOOrderEntryWithNonPrimaryDacView1>
	{
		public PXSelect<TestDac> TestView;

		public PXAction<SOOrder> Action1;

		public PXAction<SOOrder> Action2;

		public PXAction<SOOrder> Action3 { get; }
	}

	public class SOOrderEntryWithNonPrimaryDacView2 : PXGraph<SOOrderEntryWithNonPrimaryDacView2, SOOrder>
	{
		public PXSelect<SOOrder> Orders;

		public PXSelect<SOTran> OrderDetails;

		public PXAction<SOOrder> Release1;

		public PXAction<SOOrder> Release2 { get; }

		public PXAction<SOOrder> Release3;
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntryWithNonPrimaryDacView : PXGraph<SOOrderEntryWithNonPrimaryDacView, SOOrder>
	{
		public PXSelect<SOOrder> Orders;

		public PXAction<SOTran> Release;
	}

	public class SOOrderEntryWithNonPrimaryDacViewExtension : PXGraphExtension<SOOrderEntryWithNonPrimaryDacView>
	{
		public PXAction<SOTran> Action;
	}
}

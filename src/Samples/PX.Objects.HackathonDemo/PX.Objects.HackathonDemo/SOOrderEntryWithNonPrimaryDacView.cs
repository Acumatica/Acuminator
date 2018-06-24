using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntryWithNonPrimaryDacView : PXGraph<SOOrderEntry>
	{
		public PXSelect<SOOrder> Orders;

		public PXAction<SOTran> Release;
	}
}

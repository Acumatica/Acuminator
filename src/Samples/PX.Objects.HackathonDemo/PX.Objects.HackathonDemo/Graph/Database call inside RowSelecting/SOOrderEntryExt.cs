using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
		protected virtual void _(Events.RowSelecting<SOOrder> e)
		{
			var setup = SelectSetup();
		}

		private SOSetup SelectSetup()
		{
			return PXSelect<SOSetup>.SelectSingleBound(Base, null);
		}
	}
}

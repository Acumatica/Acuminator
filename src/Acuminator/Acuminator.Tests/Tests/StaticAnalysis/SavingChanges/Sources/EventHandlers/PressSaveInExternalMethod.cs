using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
	{
		protected virtual void ARInvoice_RowSelected(PXCache sender, PXRowSelectingEventArgs e)
		{
			GraphHelper.SaveChanges(this);
		}
	}

	public static class GraphHelper
	{
		public static void SaveChanges(PXGraph graph)
		{
			graph.Actions.PressSave();
		}
	}

	public class SOInvoice : IBqlTable
	{
	}
}
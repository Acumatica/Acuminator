using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
	{
		public ARInvoice GetInvoice(PXGraph graph, string refNbr)
		{
			var invoiceEntry = PXGraph.CreateInstance<ARInvoiceEntry>();
			var serviceGraph = new PXGraph();

			var invoice = PXSelect<ARInvoice, Where<ARInvoice.refNbr, Equal<Required<ARInvoice.refNbr>>>>
				.Select(graph, refNbr);
		}
	}

	public class ARInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion
	}
}
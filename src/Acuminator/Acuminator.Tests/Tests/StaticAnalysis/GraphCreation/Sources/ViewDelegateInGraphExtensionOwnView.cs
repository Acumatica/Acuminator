using PX.Data;
using System.Collections;

namespace PX.Objects
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public PXSelect<SOInvoice> Invoices;

        public IEnumerable invoices()
        {
            SOInvoiceEntry so = PXGraph.CreateInstance<SOInvoiceEntry>();

            return new PXSelect<SOInvoice>(so).Select();
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry>
    {
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
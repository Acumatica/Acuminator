using PX.Data;
using System.Collections;

namespace PX.Objects
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public IEnumerable invoices()
        {
            SOInvoiceEntry so = PXGraph.CreateInstance<SOInvoiceEntry>();

            return new PXSelect<SOInvoice>(so).Select();
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXSelect<SOInvoice> Invoices;
        public PXAction<SOInvoice> Release;

        public IEnumerable invoices()
        {
            return new PXSelect<SOInvoice>(this).Select();
        }
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
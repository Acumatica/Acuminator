using PX.Data;
using System.Collections;

namespace PX.Objects
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public IEnumerable invoices()
        {
            throw new PXSetupNotEnteredException("Setup not entered", typeof(SOInvoice));
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXSelect<SOInvoice> Invoices;

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
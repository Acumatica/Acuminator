using PX.Data;

namespace PX.Objects
{
    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public SOInvoiceEntry()
        {
            Cancel.Press();
        }
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
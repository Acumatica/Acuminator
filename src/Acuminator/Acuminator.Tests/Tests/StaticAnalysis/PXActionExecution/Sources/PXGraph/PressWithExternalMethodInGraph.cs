using PX.Data;

namespace PX.Objects
{
    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXAction<SOInvoice> Release;

        public SOInvoiceEntry()
        {
            ExecuteRelease();
        }

        private void ExecuteRelease()
        {
            Release.Press();
        }
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
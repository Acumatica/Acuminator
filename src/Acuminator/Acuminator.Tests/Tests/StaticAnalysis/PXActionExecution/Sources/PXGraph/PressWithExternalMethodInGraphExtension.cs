using PX.Data;

namespace PX.Objects
{
    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public override void Initialize()
        {
            ExecuteRelease();
        }

        private void ExecuteRelease()
        {
            Base.Release.Press();
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXAction<SOInvoice> Release;
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
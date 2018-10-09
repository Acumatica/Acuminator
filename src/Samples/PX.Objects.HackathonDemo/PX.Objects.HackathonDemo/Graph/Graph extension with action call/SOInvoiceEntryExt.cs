using PX.Data;

public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
{
    public override void Initialize()
    {
        Base.Release.Press();
    }
}

public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
{
    public PXSelect<SOInvoice> Invoices;

    public PXAction<SOInvoice> Release;
}

public class SOInvoice : IBqlTable
{
    [PXDBString(8, IsKey = true, InputMask = "")]
    public string RefNbr { get; set; }
    public abstract class refNbr : IBqlField { }
}
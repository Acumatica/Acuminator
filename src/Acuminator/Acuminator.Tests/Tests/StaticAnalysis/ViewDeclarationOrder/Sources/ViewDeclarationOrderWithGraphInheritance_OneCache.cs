using PX.Data;

public class ARInvoice : IBqlTable
{
}

public class SOInvoice : ARInvoice
{
}

public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
{
	public PXSelect<SOInvoice> SOInvoicesInBase;  // There is one cache instances: PXCache<SOInvoice>

	public PXSelect<ARInvoice> ARInvoices;
}

public class SOInvoiceEntry : ARInvoiceEntry
{
	public PXSelect<SOInvoice> SOInvoices;  // There is one cache instances: PXCache<SOInvoice>
}
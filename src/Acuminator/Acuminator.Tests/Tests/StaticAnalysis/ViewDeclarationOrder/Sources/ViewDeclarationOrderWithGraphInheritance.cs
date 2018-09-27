using PX.Data;

public class ARInvoice : IBqlTable
{
}

public class SOInvoice : ARInvoice
{
}


public class ARTran : IBqlTable { }

public class SOTran : ARTran { }

public class ARInvoiceEntry : PXGraph<ARInvoiceEntry, ARInvoice>
{
    public PXSelect<ARInvoice> ARInvoices;

	public PXSelect<SOTran> SOTrans;
}

public class SOInvoiceEntry : ARInvoiceEntry
{
	public PXSelect<SOInvoice> SOInvoices;  // There are two cache instances: PXCache<ARInvoice> and PXCache<SOInvoice>

	public PXSelect<ARTran> ARTrans;  // There is one cache : PXCache<ARTran>
}
using PX.Data;

public class BAccount : IBqlTable { }
public class Vendor : BAccount { }
public class Customer : BAccount { }

public class VendorMaint : PXGraph<VendorMaint, Vendor>
{
    public PXSelect<Vendor> Vendor;
    public PXSelect<BAccount> VendorBAcc;

    // There is only one cache instance: PXCache<Vendor>
}

public class CustomerMaint : PXGraph<CustomerMaint, Customer>
{
    public PXSelect<BAccount> CustomerBAcc;
    public PXSelect<Customer> Customer;

    // There are two cache instances: PXCache<BAccount> and PXCache<Customer>
}
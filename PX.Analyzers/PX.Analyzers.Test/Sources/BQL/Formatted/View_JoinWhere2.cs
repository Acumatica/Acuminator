using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		public PXSelectReadonly2<
			SupplierProduct,
			InnerJoin<Supplier, 
				On<Supplier.supplierID, Equal<SupplierProduct.supplierID>>>,
			Where2<
				Where<Current<SupplierFilter.countryCD>, IsNull, Or<Supplier.countryCD, Equal<Current<SupplierFilter.countryCD>>>>, And<
				Where<Current<SupplierFilter.minOrderQty>, IsNull, Or<SupplierProduct.minOrderQty, GreaterEqual<Current<SupplierFilter.minOrderQty>>>>>>,
			OrderBy<
				Asc<SupplierProduct.productID,
				Asc<SupplierProduct.supplierPrice,
				Desc<SupplierProduct.lastPurchaseDate>>>>>
			SupplierProducts;
	}
}

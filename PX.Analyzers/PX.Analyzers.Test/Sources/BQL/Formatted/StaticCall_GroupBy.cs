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
		public PXSelect<
			SupplierProduct> 
			Products;

		public IEnumerable products()
		{
			var filteredProducts = PXSelectGroupByOrderBy<
				SupplierProduct,
				InnerJoin<Supplier, 
					On<Supplier.supplierID, Equal<SupplierProduct.supplierID>>>,
				Aggregate<
					GroupBy<SupplierProduct.productID,
					GroupBy<SupplierProduct.supplierID,
						Avg<SupplierProduct.supplierPrice,
						Min<SupplierProduct.minOrderQty,
						Max<SupplierProduct.lastPurchaseDate>>>>>>,
				OrderBy<
					Asc<SupplierProduct.productID,
					Asc<SupplierProduct.supplierID>>>>
				.Select(this);
			return filteredProducts;
		}
	}
}

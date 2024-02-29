using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	public class GraphWithSetupViews : PXGraph<GraphWithSetupViews>
	{
		[PXHidden]
		public PXSetup<CRSetup> Setup;

		public PXSetupSelect<CRSetup> Setup2;

		public PXSetup<VendorClass, Where<VendorClass.vendorClassID, Equal<Current<Vendor.vendorClassID>>>> vendorclass;

		public PXSetup<Branch>.Where<Branch.branchID.IsEqual<APInvoice.branchID.AsOptional>> branch;

		public PXSetup<Branch,
			InnerJoin<INSite,
				On<INSite.branchID, Equal<Branch.branchID>>>,
			Where<INSite.siteID, Equal<Optional<SOOrder.destinationSiteID>>>> Company;

		[PXHidden]
		public PXSetupOptional<SOSetup> sosetup;

		public PXSetupOptional<INScanSetup, Where<INScanSetup.branchID.IsEqual<AccessInfo.branchID.FromCurrent>>> INSetup;
	}
}

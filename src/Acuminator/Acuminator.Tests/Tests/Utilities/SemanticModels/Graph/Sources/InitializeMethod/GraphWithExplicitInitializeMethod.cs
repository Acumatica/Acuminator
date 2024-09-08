using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.DependencyInjection;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	public class GraphWithInitialization : PXGraph<GraphWithInitialization>, IGraphWithInitialization
	{
		[PXHidden]
		public PXSetup<CRSetup> Setup;

		public PXSetup<Branch,
			InnerJoin<INSite,
				On<INSite.branchID, Equal<Branch.branchID>>>,
			Where<INSite.siteID, Equal<Optional<SOOrder.destinationSiteID>>>> Company;


		void IGraphWithInitialization.Initialize()
		{
			
		}
	}
}

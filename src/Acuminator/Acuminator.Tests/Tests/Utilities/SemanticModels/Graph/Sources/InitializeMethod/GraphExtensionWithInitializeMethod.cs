using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;
using PX.Data.DependencyInjection;
using PX.Objects.AP;
using PX.Objects.CR;
using PX.Objects.GL;
using PX.Objects.IN;
using PX.Objects.SO;

namespace Acuminator.Tests.Tests.Utilities.SemanticModels.Graph.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class GraphWithInitializationExt : PXGraphExtension<GraphWithInitialization>
	{
		public override void Initialize()
		{

		}
	}

	public class GraphWithInitialization : PXGraph<GraphWithInitialization>, IGraphWithInitialization
	{
		[PXHidden]
		public PXSetup<CRSetup> Setup;

		public PXSetup<Branch,
			InnerJoin<INSite,
				On<INSite.branchID, Equal<Branch.branchID>>>,
			Where<INSite.siteID, Equal<Optional<SOOrder.destinationSiteID>>>> Company;


		public void Initialize()
		{
			
		}
	}
}

using PX.Data;
using PX.Common;
using System;

namespace PX.Objects.HackathonDemo.Graph.InternalApiUsage
{
    public class SOOrderMaintSync : PXGraph<SOOrderMaintSync> 
    {
        private readonly ServiceProvider serviceProvider = new ServiceProvider();

        public int CountSyncReadyFiles(int filesNbr = 0)
        {
            if (WebConfig.IsClusterEnabled)
			{

			}

            return filesNbr;
        }

        public void InvalidateCache()
        {
			if (serviceProvider.AccessChecker != null && serviceProvider.AccessChecker.IsActive && serviceProvider.AccessChecker.ShouldCheck)
			{
                serviceProvider.AccessChecker.CheckAccess();
			}  

            if (serviceProvider.Service != null && serviceProvider.Service.SomeFlag && serviceProvider.Service.IsActive)
			{
                serviceProvider.Service.ProvideService();
			}


            PX.Api.Mobile.Legacy.Provider.InvalidateCache();

        }
    }
}

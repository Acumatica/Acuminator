using PX.Data;
using PX.Common;
using System;

namespace PX.Objects.HackathonDemo.Graph.InternalApiUsage
{
    public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
    {
        public int CountSyncReadyFiles(int filesNbr = 0)
        {
            if (WebConfig.IsClusterEnabled)
			{

			}

            return filesNbr;
        }
    }
}

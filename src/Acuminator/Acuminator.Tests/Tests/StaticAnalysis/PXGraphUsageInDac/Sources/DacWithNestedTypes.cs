using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    public class SOOrder : IBqlTable
    {
	    private class SOShipmentNbrAttribute : PXIntAttribute
	    {
		    public SOShipmentNbrAttribute()
		    {
				SOOrderMaintSync graph = PXGraph.CreateInstance<SOOrderMaintSync>();
			}
		}

	    public class SOLine : IBqlTable
	    {
		    [PXDBInt]
		    public int? Count
		    {
			    get
			    {
					SOOrderMaintSync graph = PXGraph.CreateInstance<SOOrderMaintSync>();
				    return graph.CountSyncReadyFiles();
			    }
		    }
	    }

        public abstract class shipmentNbr : IBqlField { }
        [PXInt]
        public virtual int? ShipmentNbr { get; set; }
	}

    public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
    {
        public int CountSyncReadyFiles()
        {
            return 0;
        }
    }
}

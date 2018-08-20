using PX.Data;
using System;

namespace PX.Objects.HackathonDemo
{
    [Serializable]
    public class SOShipmentExt : PXCacheExtension<SOOrder>
    {
        public abstract class shipmentNbrExt : IBqlField { }
        [PXInt]
        [PXUIField(DisplayName = "Shipment Number")]
        public virtual int? ShipmentNbrExt
        {
            get
            {
                SOOrderMaintSync graph = PXGraph.CreateInstance<SOOrderMaintSync>();

                return graph.CountSyncReadyFiles();
            }
        }

        public static void UpdateReadyFilesCount()
        {
            SOOrderMaintSync syncGraph = PXGraph.CreateInstance<SOOrderMaintSync>();
            syncGraph.CountSyncReadyFiles();
        }
    }

    public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
    {
        public int CountSyncReadyFiles(int filesNbr = 0)
        {
            return filesNbr;
        }
    }
}

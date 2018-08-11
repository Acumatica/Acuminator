using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    [Serializable]
    public class SOOrder : IBqlTable
    {
        public abstract class shipmentNbr : IBqlField { }
        [PXInt]
        public virtual int? ShipmentNbr
        {
            get
            {
                SOOrderMaintSync graph = PXGraph.CreateInstance<SOOrderMaintSync>();

                return graph.CountSyncReadyFiles();
            }
        }

        public abstract class shipmentNbr2 : IBqlField { }
        [PXInt]
        public virtual int? ShipmentNbr2
        {
            get
            {
                return SOOrderSyncHelper.Graph.CountSyncReadyFiles();
            }
        }

        public abstract class shipmentNbr3 : IBqlField { }
        [PXInt]
        public virtual int? ShipmentNbr3
        {
            get
            {
                return syncGraph.CountSyncReadyFiles();
            }
        }

        private SOOrderMaintSync syncGraph = PXGraph.CreateInstance<SOOrderMaintSync>();

        public static void Update()
        {
            SOOrderMaintSync syncGraph = PXGraph.CreateInstance<SOOrderMaintSync>();
            syncGraph.CountSyncReadyFiles();
        }
    }

    public class SOOrderMaintSync : PXGraph<SOOrderMaintSync>
    {
        public int CountSyncReadyFiles()
        {
            return 0;
        }
    }

    public class SOOrderSyncHelper
    {
        public static SOOrderMaintSync Graph { get; } = PXGraph.CreateInstance<SOOrderMaintSync>();
    }
}

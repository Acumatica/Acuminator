using PX.Data;
using System;

namespace Acuminator.Tests.Sources
{
    public class SOOrder : IBqlTable
    {
	    private class SOShipmentNbrAttribute : PXIntAttribute
	    {
		    protected void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		    {
		    }
		}

        public abstract class shipmentNbr : IBqlField { }
        [PXInt]
        public virtual int? ShipmentNbr { get; set; }
	}
}

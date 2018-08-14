using PX.Data;
using System;

namespace PX.Objects.HackathonDemo
{
    public class SOOrderEntryWithCustomText : PXGraphExtension<SOOrderEntry>
    {
        public void SOOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            if (!(e.Row is SOOrder row))
                return;

            SOOrderExt rowExt = sender.GetExtension<SOOrderExt>(row);

            if (rowExt == null)
                return;

            if (!row.OrderNbr.Equals(SOOrderExt.SpecialOrderNbr, StringComparison.Ordinal))
                return;

            string newDisplayName = PXLocalizer.Localize("Special Text");
            PXUIFieldAttribute.SetDisplayName<SOOrderExt.customText>(sender, newDisplayName);
        }
    }
}

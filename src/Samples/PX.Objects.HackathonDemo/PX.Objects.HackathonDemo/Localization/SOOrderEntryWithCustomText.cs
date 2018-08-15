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

            string newDisplayName = null;

            if (row.OrderNbr.Equals(SOOrderExt.SpecialOrderNbr, StringComparison.Ordinal))
            {
                newDisplayName = PXLocalizer.Localize("Special Text");
                PXUIFieldAttribute.SetDisplayName<SOOrderExt.customText>(sender, newDisplayName);
            }
            else if (row.OrderNbr.Equals(SOOrderExt.SpecialOrderNbr2, StringComparison.Ordinal))
            {
                newDisplayName = PXLocalizer.Localize(Messages.SpecialText, typeof(Messages).FullName);
                PXUIFieldAttribute.SetDisplayName<SOOrderExt.customText>(sender, newDisplayName);
            }

            if (string.IsNullOrEmpty(newDisplayName))
                return;

            PXUIFieldAttribute.SetDisplayName<SOOrderExt.customText>(sender, newDisplayName);
        }
    }
}

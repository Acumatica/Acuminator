using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions.Sources
{
	public class SOInvoiceDerivedEntry : SOInvoiceCustomEntry
	{
		protected override void SOInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)	// Should not show PX1041
		{
			
		}
	}
}
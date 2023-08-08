using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceCustomEntry>
	{
		[PXOverride]
		public virtual void SOInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)		// Should not show PX1041
		{
			
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Analyzers.StaticAnalysis.NameConventionEventsInGraphsAndGraphExtensions.Sources
{
	public class SOInvoiceCustomEntry : PXGraph<SOInvoiceCustomEntry>
	{
		protected virtual void SOInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
		{
			
		}
	}

	[PXCacheName("SO Invoice")]
	public class SOInvoice : IBqlTable
	{
		#region RefNbr
		[PXDBString(8, IsKey = true, InputMask = "")]
		public string RefNbr { get; set; }
		public abstract class refNbr : IBqlField { }
		#endregion	
	}
}
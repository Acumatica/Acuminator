using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class BinExtension : PXGraphExtension<INSiteMaint>
	{
		public void INLocation_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			if (e.TranStatus != PXTranStatus.Completed) return;

			var graph = new PXGraph();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects;

namespace PX.Objects.Ext
{
	public class ARReleaseProcess : PXGraph<ARReleaseProcess>
	{
		public PXSelect<ARInvoice> Documents;
	}

	public class ARReleaseProcess_Extension : PXGraphExtension<ARReleaseProcess>
	{
		public ARReleaseProcess_Extension()
		{
			Base.Documents.AllowInsert = false;
			Base.Documents.AllowUpdate = false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	class NonSpecificPXGraphCreateInstanceMethod : IPrefetchable
	{
		public void Prefetch()
		{
			var graph = new PXGraph();
		}
	}
}

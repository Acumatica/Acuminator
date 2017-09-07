using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	class PX1001ClassWithProperty
	{
		public PXGraph Graph
		{
			get { return new PX1001PropertyGraph(); }
		}
	}

	class PX1001PropertyGraph : PXGraph<PX1001PropertyGraph>
	{
	}
}

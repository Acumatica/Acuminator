using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class SomeBaseGraph : PXGraph<SomeBaseGraph>
	{
		public PXSelect<SomeDocument> Adjustments;

		public virtual IEnumerable adjustments()
		{
			yield break;
		}
	}

	public class SomeDerivedGraph : SomeBaseGraph
	{
		public PXSelect<SomeDocument> SOAdjustments;

		public override IEnumerable adjustments()
		{
			yield break;
		}
	}

	public class SomeDocument : IBqlTable
	{
	}
}

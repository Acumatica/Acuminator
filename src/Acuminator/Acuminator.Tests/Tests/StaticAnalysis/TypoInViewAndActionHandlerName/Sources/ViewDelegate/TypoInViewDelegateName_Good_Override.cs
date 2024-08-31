using System;
using System.Collections;
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

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExt : PXGraphExtension<SomeBaseGraph>
	{
		public PXSelect<SomeDocument> SOAdjustments;

		public IEnumerable adjustments()
		{
			yield break;
		}
	}

	[PXHidden]
	public class SomeDocument : IBqlTable
	{
	}
}

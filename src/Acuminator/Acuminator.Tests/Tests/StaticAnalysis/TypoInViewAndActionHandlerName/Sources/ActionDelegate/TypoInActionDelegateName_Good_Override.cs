using System;
using System.Collections;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class SomeBaseGraph : PXGraph<SomeBaseGraph>
	{
		public PXAction<SomeDocument> Release;

		[PXButton]
		[PXUIField]
		public virtual IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}
	}

	public class SomeDerivedGraph : SomeBaseGraph
	{
		public PXAction<SomeDocument> SORelease;

		[PXButton]
		[PXUIField]
		public override IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SomeGraphExt : PXGraphExtension<SomeBaseGraph>
	{
		public PXAction<SomeDocument> SORelease;

		[PXButton]
		[PXUIField]
		public IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}
	}

	[PXHidden]
	public class SomeDocument : IBqlTable
	{
	}
}

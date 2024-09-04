using System;
using System.Collections;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public PXAction<Dac> Release;

		[PXButton]
		[PXUIField]
		public IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}
	}

	[PXHidden]
	public class Dac : IBqlTable
	{
	}
}

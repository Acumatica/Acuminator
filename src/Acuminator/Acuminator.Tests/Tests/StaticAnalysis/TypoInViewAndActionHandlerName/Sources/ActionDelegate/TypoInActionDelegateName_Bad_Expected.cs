using System;
using System.Collections;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class MyGraph : PXGraph<MyGraph>
	{
		public PXAction<MyDac> Release;
		public PXAction<MyDac> ViewOriginalDocument;

		[PXButton]
		[PXUIField]
		public IEnumerable release(PXAdapter adapter)
		{
			yield break;
		}

		[PXButton]
		[PXUIField]
		public IEnumerable viewOriginalDocument(PXAdapter adapter)
		{
			yield break;
		}
	}

	public class MyDac : IBqlTable
	{
	}
}

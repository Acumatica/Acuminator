using System;
using System.Collections;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DAC : IBqlTable
	{
	}

	public class SimpleGraph : PXGraph<SimpleGraph>
	{
		public PXSelect<DAC> ViewInBaseGraph;
	}

	public class DerivedGraph : SimpleGraph
	{
		public PXSelect<DAC> Documents;
		public PXSelect<DAC> CurrentDocument;

		public IEnumerable documents()
		{
			yield break;
		}

		public IEnumerable currentDocument()
		{
			yield return Documents.Current;
		}

		public IEnumerable viewInBaseGraph()
		{
			yield break;
		}
	}
}

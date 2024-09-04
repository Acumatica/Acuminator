using System;
using System.Collections;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class MyDac : IBqlTable
	{
	}

	public class SimpleGraph : PXGraph<SimpleGraph>
	{
		public PXAction<MyDac> ActionInBaseGraph;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class DerivedGraph : SimpleGraph
	{
		public PXAction<MyDac> ReleaseDocuments;
		public PXAction<MyDac> ViewCurrentDocument;

		[PXButton]
		[PXUIField]
		public IEnumerable releaseDocumentss(PXAdapter adapter)
		{
			yield break;
		}

		[PXButton]
		[PXUIField]
		public IEnumerable viewCurrentDocument(PXAdapter adapter)
		{
			yield break;
		}

		[PXButton]
		[PXUIField]
		public IEnumerable actionInBasGraph(PXAdapter adapter)
		{
			yield break;
		}
	}
}

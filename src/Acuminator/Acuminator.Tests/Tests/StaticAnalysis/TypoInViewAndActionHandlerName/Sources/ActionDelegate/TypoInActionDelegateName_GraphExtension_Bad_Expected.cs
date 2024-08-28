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
	public class SimpleGraphExtension : PXGraphExtension<SimpleGraph>
	{
		public PXAction<MyDac> ReleaseDocuments;
		public PXAction<MyDac> ViewCurrentDocument;

		[PXButton]
		[PXUIField]
		public IEnumerable releaseDocuments(PXAdapter adapter)
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
		public IEnumerable actionInBaseGraph(PXAdapter adapter)
		{
			yield break;
		}
	}
}

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

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SimpleGraphExtension : PXGraphExtension<SimpleGraph>
	{
		public PXSelect<DAC> Documents;
		public PXSelect<DAC> CurrentDocument;

		public IEnumerable documentss()
		{
			yield break;
		}

		public IEnumerable currentDocument()
		{
			yield return Documents.Current;
		}

		public IEnumerable viewInBasGraph()
		{
			yield break;
		}
	}
}

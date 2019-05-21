using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class DAC : IBqlTable
	{
	}

	public class SimpleGraph : PXGraph<SimpleGraph>
	{

	}

	public class SimpleGraphExtension : PXGraphExtension<SimpleGraph>
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
	}	
}

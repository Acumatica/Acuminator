using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class TypoInViewDelegateName_Good : PXGraph<TypoInViewDelegateName_Good>
	{
		public PXSelect<TypoInViewDelegateName_Good_DAC> Documents;

		public IEnumerable someDocuments()
		{
			yield break;
		}
	}

	public class TypoInViewDelegateName_Good_DAC : IBqlTable
	{
	}
}

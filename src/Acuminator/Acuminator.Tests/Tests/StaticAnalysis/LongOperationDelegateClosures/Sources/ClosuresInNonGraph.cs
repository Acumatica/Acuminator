using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	[PXHidden]
	public class SomeDAC : IBqlTable
	{
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{
		PXProcessing<SomeDAC> _processing;

		PXAction<SomeDAC> SomeAction;

		public IEnumerable someAction(PXAdapter adapter)
		{
			var helper = new NonGraph();
			helper.RunLongRun(_processing);

			return adapter.Get();
		}
	}


	public class NonGraph
	{
		private static readonly Guid ID = Guid.NewGuid();

		public virtual void RunLongRun(PXProcessing<SomeDAC> processingView)
		{
			processingView.SetProcessDelegate((SomeDAC item) => MemberFunc());

			PXLongOperation.StartOperation(ID, () => MemberFunc());
		}

		public void MemberFunc()
		{ }
	}
}
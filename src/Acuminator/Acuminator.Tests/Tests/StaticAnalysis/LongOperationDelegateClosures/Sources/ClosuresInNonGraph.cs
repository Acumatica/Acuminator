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
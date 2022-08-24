using System;
using PX.Data;

using System.Collections;
using System.Collections.Generic;

namespace PX.Objects.HackathonDemo
{
	public class NonGraph
	{
		private static readonly Guid ID = Guid.NewGuid();

		public virtual void RunLongRun(PXProcessing<SOItemProcessing.SomeDAC> processingView)
		{
			processingView.SetProcessDelegate((SOItemProcessing.SomeDAC item) => MemberFunc());

			PXLongOperation.StartOperation(ID, () => MemberFunc());
		}

		public void MemberFunc()
		{ }
	}
}
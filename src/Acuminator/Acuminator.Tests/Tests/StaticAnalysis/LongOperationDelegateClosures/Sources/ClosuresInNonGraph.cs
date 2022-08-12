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
			helper.RunLongRunWithoutCapture(_processing);					 // No diagnostic					
			helper.SetProcessingWithGraphCapture(_processing, this);		 // Show diagnostic
			helper.SetProcessingWithAdapterCapture(_processing, adapter);	 // Show diagnostic

			return adapter.Get();
		}
	}


	public class NonGraph
	{
		private static readonly Guid ID = Guid.NewGuid();

		public virtual void RunLongRunWithoutCapture(PXProcessing<SomeDAC> processingView)
		{
			processingView.SetProcessDelegate((SomeDAC item) => MemberFunc());      // No diagnostic

			PXLongOperation.StartOperation(ID, () => MemberFunc());					// No diagnostic
		}

		public virtual void SetProcessingWithGraphCapture(PXProcessing<SomeDAC> processingView, PXGraph graph)
		{
			processingView.SetProcessDelegate((SomeDAC item) =>                         // No diagnostic
			{
				if (graph.IsMobile)
				{
					MemberFunc();
				} 		
			});
		}

		public virtual void SetProcessingWithAdapterCapture(PXProcessing<SomeDAC> processingView, PXAdapter adapterToCapture)
		{
			processingView.SetProcessDelegate((SomeDAC item) =>                         // No diagnostic
			{
				if (adapterToCapture != null)
					MemberFunc();
			});
		}

		public void MemberFunc()
		{ }
	}
}
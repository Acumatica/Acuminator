using PX.Data;

using System;
using System.Collections;
using System.Collections.Generic;

using static Microsoft.VisualStudio.Threading.AsyncReaderWriterLock;

namespace Acuminator.Tests.Sources
{
	public class ProcessingGraph : PXGraph<ProcessingGraph>
	{
		[PXHidden]
		public class SomeDAC : IBqlTable
		{
		}

		public PXAction<SomeDAC> SomeAction;

		[PXUIField(DisplayName = "Some Action", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable someAction(PXAdapter adapter)
		{
			var localGraph = PXGraph.CreateInstance<ProcessingGraph>();

			PXLongOperation.StartOperation(this, InstanceMember);           // Show diagnostic

			CaptureGraph_PassedDelegate(InstanceMember);                    // Show diagnostic
			CaptureGraph_PassedDelegate(localGraph.InstanceMember);         // No diagnostic
			CaptureGraph_PassedDelegate(StaticMember);                      // No diagnostic

			CaptureGraph_RecursivePassDelegate(InstanceMember);             // Show diagnostic
			CaptureGraph_RecursivePassDelegate(localGraph.InstanceMember);  // No diagnostic
			CaptureGraph_RecursivePassDelegate(StaticMember);               // No diagnostic

			NoCaptureGraph_RecursivePassDelegate(this);                     // No diagnostic

			CaptureGraph_PassedDelegate(this);								// Show diagnostic
			CaptureGraph_PassedDelegate(localGraph);                        // No diagnostic

			CaptureGraph_RecursivePassDelegate(this);						// Show diagnostic
			CaptureGraph_RecursivePassDelegate(localGraph);					// No diagnostic

			return adapter.Get();
		}

		private void NoCaptureGraph_RecursivePassDelegate(ProcessingGraph graph) =>
			CaptureGraph_PassedDelegate(StaticMember);

		private void CaptureGraph_RecursivePassDelegate(ProcessingGraph graph) =>
			CaptureGraph_PassedDelegate(graph.InstanceMember);

		private void CaptureGraph_RecursivePassDelegate(PXToggleAsyncDelegate action) =>
			CaptureGraph_PassedDelegate(action);

		private void CaptureGraph_PassedDelegate(ProcessingGraph graph)
		{
			CaptureGraph_PassedDelegate(graph.InstanceMember);
		}

		private void CaptureGraph_PassedDelegate(PXToggleAsyncDelegate action)
		{
			PXLongOperation.StartOperation(this, action);
		}

		private void InstanceMember()
		{

		}

		private static void StaticMember()
		{

		}
	}
}
using PX.Common;
using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.CR;
using PX.Objects.CR.Extensions.PinActivity;
using PX.Objects.EP;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Acuminator.Tests.Sources
{
	public class CRActivityPinnedViewAttribute : PXViewExtensionAttribute
	{
		public override void ViewCreated(PXGraph graph, string viewName)
		{
			PXNamedAction.AddAction(graph, graph.PrimaryItemType, nameof(TogglePinActivity), "Pin or Unpin",
									adapter => TogglePinActivity(graph, viewName, adapter));
		}

		public void StartLongRun()
		{
			PXLongOperation.StartOperation(new Guid(), MemberFunc);                    // No diagnostic
			PXLongOperation.StartOperation(new Guid(), () => MemberFunc());            // No diagnostic
		}


		[PXButton]
		public virtual IEnumerable TogglePinActivity(PXGraph graph, string viewName, PXAdapter adapter)
		{
			PXLongOperation.StartOperation(adapter.View.Graph, MemberFunc);                 // No diagnostic
			PXLongOperation.StartOperation(adapter.View.Graph, () => adapter.Get());        // Show diagnostic

			Run(adapter);                                                                   // Show diagnostic
			Run(null);

			return adapter.Get();
		}

		private static void Run(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(adapter.View.Graph, () => adapter.Get());        // No diagnostic
		}

		private void MemberFunc()
		{

		}
	}
}
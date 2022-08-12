using PX.Common;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.EP;

using System;
using System.Collections;
using System.Collections.Generic;

using static Microsoft.VisualStudio.Threading.AsyncReaderWriterLock;

namespace Acuminator.Tests.Sources
{
	public class CRRelationsList<TNoteField> : PXSelect<CRRelation>
		where TNoteField : IBqlField
	{
		public CRRelationsList(PXGraph graph)
			: base(graph, GetHandler())
		{
			AttacheEventHandlers(graph);
		}
		private void AttacheEventHandlers(PXGraph graph)
		{
			PXDBDefaultAttribute.SetSourceType<CRRelation.refNoteID>(graph.Caches[typeof(CRRelation)], typeof(TNoteField));
			
			graph.Initialized +=
				sender =>
				{
					AppendActions(graph);
				};
		}

		private void AppendActions(PXGraph graph)
		{
			var viewName = graph.ViewNames[View];
			var primaryDAC = BqlCommand.GetItemType(typeof(TNoteField));
			PXNamedAction.AddAction(graph, primaryDAC, viewName + "Action", null, false, ActionHandler);
		}

		public void StartLongRun()
		{
			PXLongOperation.StartOperation(_Graph, MemberFunc);								// Show diagnostic
			PXLongOperation.StartOperation(_Graph, () => MemberFunc());						// Show diagnostic
		}

		[PXButton]
		private IEnumerable ActionHandler(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(adapter.View.Graph, MemberFunc);                 // Show diagnostic
			PXLongOperation.StartOperation(adapter.View.Graph, () => adapter.Get());        // Show diagnostic

			Run(adapter);                                                                   // Show diagnostic
			Run(null);																		// No diagnostic

			return adapter.Get();
		}

		private static void Run(PXAdapter adapter)
		{
			PXLongOperation.StartOperation(adapter.View.Graph, () => adapter.Get());		// No diagnostic
		}

		private void MemberFunc()
		{

		}

		private static PXSelectDelegate GetHandler() =>
			() => new List<object>();

	}
}
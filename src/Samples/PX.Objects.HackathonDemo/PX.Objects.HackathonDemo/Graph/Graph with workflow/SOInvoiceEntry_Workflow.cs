using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.AR;

namespace PX.Objects.HackathonDemo
{
	public class ChangeOrdersEntry : PXGraph<ChangeOrdersEntry>
	{
		public override void Configure(PXScreenConfiguration graph) => base.Configure(graph);
	}

	public class SOInvoiceEntry_Workflow : PXGraphExtension<SOInvoiceEntry>
	{
		[PXWorkflowDependsOnType(typeof(ARSetup))]
		public sealed override void Configure(PXScreenConfiguration config) =>
			Configure(config.GetScreenConfigurationContext<SOInvoiceEntry, ARInvoice>());

		protected static void Configure(WorkflowContext<SOInvoiceEntry, ARInvoice> context)
		{
		}
	}
}

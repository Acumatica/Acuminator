using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.WorkflowAPI;
using PX.Objects.SO;

namespace Acuminator.Tests.Tests.StaticAnalysis.NoIsActiveMethodForExtension.Sources
{
	public class SOOrderEntryWorkflow : SOOrderEntryWorkflowBase
	{
		public PXSelect<SOOrder> Orders;

		protected override void Configure(WorkflowContext<SOOrderEntry, SOOrder> workflowContext) => 
			base.Configure(workflowContext);
	}

	public class SOOrderEntryWorkflowBase : PXGraphExtension<SOOrderEntry>
	{
		protected virtual void _(Events.RowDeleted<SOOrder> e)
		{
		}

		public override void Configure(PXScreenConfiguration configuration)
		{
			var workflowContext = configuration.GetScreenConfigurationContext<SOOrderEntry, SOOrder>();
			Configure(workflowContext);
		}

		protected virtual void Configure(WorkflowContext<SOOrderEntry, SOOrder> workflowContext)
		{

		}
	}

	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{

	}
}
using PX.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderEntryExt : PXGraphExtension<SOOrderEntry>
	{
		protected virtual void _(Events.RowSelecting<SOOrder> e)
		{
			var setup = SelectSetup();
			Base.Caches[typeof(SOSetup)].Insert(setup);
		}

		protected virtual void SOOrder_OrderNbr_CacheAttached(PXCache cache)
		{
			cache.Graph.RowSelecting.AddHandler<SOOrder>((sender, args) => PXDatabase.SelectTimeStamp());
		}

		private SOSetup SelectSetup()
		{
			return PXSelect<SOSetup>.SelectSingleBound(Base, null);
		}

		protected virtual void _(Events.RowInserted<SOOrder> e)
		{
			var graph = PXGraph.CreateInstance<SOOrderEntry>();
			graph.Orders.Current = e.Row;
			PXLongOperation.StartOperation(graph, () => graph.Release.Press());
		}

		protected virtual void _(Events.RowUpdated<SOOrder> e)
		{
			Base.Persist();
		}

		protected virtual void _(Events.RowSelected<SOOrder> e)
		{
			Base.Actions.PressSave();
		}
	}
}

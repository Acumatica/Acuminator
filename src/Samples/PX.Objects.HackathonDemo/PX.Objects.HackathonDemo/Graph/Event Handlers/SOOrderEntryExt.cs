﻿using PX.Data;
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
            Base.Release.SetEnabled(e.Row.OrderBal > 0);
            PXUIFieldAttribute.SetEnabled<SOOrder.orderType>(e.Cache, e.Row, false);

			var setup = SelectSetup();

            if (setup == null)
                throw new PXSetupNotEnteredException<SOSetup>("Setup is not entered");

			Base.Caches[typeof(SOSetup)].Insert(setup);

            if (e.Row.OrderBal < 0)
                e.Cache.RaiseExceptionHandling<SOOrder.orderBal>(e.Row, e.Row.OrderBal, new PXSetPropertyException("Negative balance"));
		}

		protected virtual void SOOrder_OrderNbr_CacheAttached(PXCache cache)
		{
			cache.Graph.RowSelecting.AddHandler<SOOrder>((sender, args) => PXDatabase.SelectTimeStamp());
		}

		private SOSetup SelectSetup()
		{
			return PXSelect<SOSetup>.SelectSingleBound(Base, null);
		}

        protected virtual void _(Events.RowInserting<SOOrder> e)
        {
            e.Row.OrderDate = DateTime.UtcNow;
            Base.Orders.Current.OrderBal = 0;
        }

		protected virtual void _(Events.RowInserted<SOOrder> e)
		{
			var graph = PXGraph.CreateInstance<SOOrderEntry>();
			graph.Orders.Current = e.Row;
			PXLongOperation.StartOperation(graph, () => graph.Release.Press());
		}

		protected virtual void SOOrder_OrderNbr_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e) => Base.Persist();

		protected virtual void SOOrder_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
            var row = e.Row as SOOrder;
			if (row != null && sender.GetStatus(row) == PXEntryStatus.Updated)
			{
				Base.Actions.PressSave();
                Base.Release.Press();
                PXDatabase.SelectTimeStamp();
			}
		}

        protected virtual void _(Events.RowPersisted<SOOrder> e)
        {
            throw new NotSupportedException();
        }
	}
}

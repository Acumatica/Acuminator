using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class CurrencyInfoAttribute : PXAggregateAttribute, 
										 IPXRowInsertingSubscriber, IPXRowPersistingSubscriber, IPXRowPersistedSubscriber, 
										 IPXRowUpdatingSubscriber, IPXReportRequiredField, IPXDependsOnFields
	{
		public ISet<Type> GetDependencies(PXCache cache)
		{
			throw new NotImplementedException();
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}

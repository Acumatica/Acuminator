using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Class | AttributeTargets.Method)]
	public class PXDBCurrencyAttribute : PXDBDecimalAttribute, IPXFieldVerifyingSubscriber, IPXRowInsertingSubscriber
	{
		public void FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void RowInserting(PXCache sender, PXRowInsertingEventArgs e)
		{
			throw new NotImplementedException();
		}

		public PXDBCurrencyAttribute(Type keyField, Type resultField)
			: base()
		{
			throw new NotImplementedException();
		}

		public PXDBCurrencyAttribute(Type precision, Type keyField, Type resultField)
			: base(precision)
		{
			throw new NotImplementedException();
		}
	}

}

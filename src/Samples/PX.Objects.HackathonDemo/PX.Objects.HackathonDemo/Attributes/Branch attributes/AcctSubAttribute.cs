using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class AcctSubAttribute : PXAggregateAttribute, IPXInterfaceField, IPXCommandPreparingSubscriber, IPXRowSelectingSubscriber
	{
		public string DisplayName { get; set; }
		public PXUIVisibility Visibility { get; set; }
		public bool Enabled { get; set; }
		public bool Visible { get; set; }
		public string ErrorText { get; set; }
		public object ErrorValue { get; set; }
		public PXErrorLevel ErrorLevel { get; set; }
		public int TabOrder { get; set; }
		public PXCacheRights MapEnableRights { get; set; }
		public PXCacheRights MapViewRights { get; set; }
		public bool ViewRights { get; }

		public void CommandPreparing(PXCache sender, PXCommandPreparingEventArgs e)
		{
			throw new NotImplementedException();
		}

		public void ForceEnabled()
		{
			throw new NotImplementedException();
		}

		public void RowSelecting(PXCache sender, PXRowSelectingEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}

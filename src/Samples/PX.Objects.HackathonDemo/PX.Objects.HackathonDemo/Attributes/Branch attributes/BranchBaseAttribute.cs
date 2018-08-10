using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXDBInt]
	[PXInt]
	[PXUIField(DisplayName = "Branch")]
	public abstract class BranchBaseAttribute : AcctSubAttribute, IPXFieldSelectingSubscriber
	{
		public BranchBaseAttribute(Type sourceType, bool addDefaultAttribute = true, bool onlyActive = false)
			: this(sourceType, null)
		{
			throw new NotImplementedException();
		}

		protected BranchBaseAttribute(Type sourceType, Type searchType, bool onlyActive = false) : base()
		{
			throw new NotImplementedException();
		}

		public BranchBaseAttribute(bool onlyActive = false)
			: this(typeof(AccessInfo.branchID), addDefaultAttribute: true, onlyActive: onlyActive)
		{
			throw new NotImplementedException();
		}

		public void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}

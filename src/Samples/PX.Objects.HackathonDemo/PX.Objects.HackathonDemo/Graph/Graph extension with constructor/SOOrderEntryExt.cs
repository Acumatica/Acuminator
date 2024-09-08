using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderBalanced : PXGraphExtension<SOOrderEntry>
	{
		public override void Initialize()
		{
			base.Initialize();
		}

		public SOOrderBalanced()
		{
			Base.Orders.AllowInsert = false;
			Base.Orders.AllowUpdate = false;
			Base.Orders.AllowDelete = false;
			Base.Orders.WhereAnd<Where<SOOrderWithHold.hold, Equal<False>>>();
		}
	}
}

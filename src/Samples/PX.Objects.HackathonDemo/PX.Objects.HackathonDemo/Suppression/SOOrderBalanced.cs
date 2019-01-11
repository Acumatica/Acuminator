using PX.Data;

namespace PX.Objects.HackathonDemo.Suppression
{
	public class SOOrderBalanced : PXGraphExtension<SOOrderEntry>
	{
		public SOOrderBalanced()
		{
			Base.Orders.AllowInsert = false;
			Base.Orders.AllowUpdate = false;
			Base.Orders.AllowDelete = false;
			Base.Orders.WhereAnd<Where<SOOrderWithHold.hold, Equal<False>>>();
		}
	}
}

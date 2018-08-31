using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;

namespace PX.Objects.SO
{
	public class SOOrder : IBqlTable
	{
		public abstract class orderType { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string OrderType { get; set; }

		public abstract class orderNbr { }
		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string OrderNbr { get; set; }
	}

	public class SOEntry : PXGraph<SOEntry>
	{
		public PXSelect<SOOrder> Documents;
	}

	public class SOEntryExt : PXGraphExtension<SOEntry>
	{
		public PXAction<SOOrder> NewOrder;

		[PXUIField(DisplayName = "New Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public IEnumerable newOrder(PXAdapter adapter)
		{
			return adapter.Get();
		}
	}
}

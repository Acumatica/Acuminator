using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo.Extensions.NonPublic
{
	internal class SOOrderExt1 : PXGraphExtension<SOOrderEntry>  //Non public graph extensions are not supported
	{ 
		public virtual void _(Events.RowUpdating<SOOrder> e)
		{

		}
	}

	class SOOrderExt2 : PXGraphExtension<SOOrderEntry>          //Non public graph extensions are not supported
	{
		public virtual void _(Events.RowUpdated<SOOrder> e)
		{

		}
	}


	static class PurchaseGlobalState
	{
		private class PurchaseEngine
		{
			public sealed class SOOrderEntryExtPurchase : PXGraphExtension<SOOrderEntry>    //Non public graph extensions are not supported
			{
				public virtual void _(Events.RowSelected<SOOrder> e)
				{

				}
			}
		}
	}
}

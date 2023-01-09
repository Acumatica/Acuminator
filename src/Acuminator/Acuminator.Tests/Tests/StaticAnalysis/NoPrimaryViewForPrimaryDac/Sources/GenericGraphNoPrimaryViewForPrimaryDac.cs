using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	[PXCacheName("ARInvoice")]
	public class ARInvoice : IBqlTable { }

	[PXCacheName("SOOrder")]
	public class SOOrder : IBqlTable { }

	public class InvoiceEntryBase<TGraph, TDac> : PXGraph<TGraph, TDac>
	where TGraph : PXGraph, new()
	where TDac : class, IBqlTable, new()
	{
	}

	public class ARInvoiceEntryBase<TGraph> : InvoiceEntryBase<TGraph, ARInvoice>	// The only place to show alert since there is no primary DAC as a type parameter and no view for ARInvoice
	where TGraph : PXGraph, new()
	{

	}

	public class ARInvoiceEntry : ARInvoiceEntryBase<ARInvoiceEntry>
	{
		public PXSelect<ARInvoice> Invoices;
	}

	public class APInvoiceEntryBase<TDac> : InvoiceEntryBase<APInvoiceEntryBase<TDac>, TDac>	// No alert since primary DAC is a generic type parameter
	where TDac : class, IBqlTable, new()
	{

	}

	//------------------------------------------------------------------------------------------

	public class OrderEntryBaseWithView<TGraph, TSomeType, TDac> : PXGraph<TGraph, TDac>
	where TGraph : PXGraph, new()
	where TDac : class, IBqlTable, new()
	{
		public PXSelect<TDac> Orders;
	}

	public class SOOrderEntryBase<TDac> : OrderEntryBaseWithView<SOOrderEntryBase<TDac>, int, TDac>
	where TDac : class, IBqlTable, new()
	{
		
	}

	public class SOOrderEntry : SOOrderEntryBase<SOOrder>
	{
	}

	public class OtherSOOrderEntry : OrderEntryBaseWithView<OtherSOOrderEntry, object, SOOrder>
	{
	}
}

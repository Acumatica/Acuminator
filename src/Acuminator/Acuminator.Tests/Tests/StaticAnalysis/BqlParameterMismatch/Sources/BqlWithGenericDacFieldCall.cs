using System;
using System.Collections.Generic;

using PX.Data;
using PX.Data.BQL;
using PX.Objects.AR;

namespace PX.Objects.HackathonDemo
{
	public abstract class SomeGraphBaseExt<TGraph, TDac, TRefNbrField, TDocTypeVal> : PXGraphExtension<TGraph>
	where TGraph : PXGraph
	where TDac : class, IBqlTable, new()
	where TRefNbrField : BqlString.Field<TRefNbrField>
	where TDocTypeVal : BqlString.Constant<TDocTypeVal>, new()
	{
		public PXSelect<ARInvoice,
					Where<ARInvoice.refNbr, Equal<Current<TRefNbrField>>,
					  And<ARInvoice.docType, Equal<TDocTypeVal>>>,
				  OrderBy<
					  Asc<ARInvoice.refNbr>>>
		   Invoices;

		protected virtual ARInvoice GetInvoice()
		{
			ARInvoice invoice = Invoices.SelectSingle();
			return invoice; 
		}

		protected virtual ARInvoice GetInvoice2()
		{
			ARInvoice invoice = PXSelect<ARInvoice,
					Where<ARInvoice.refNbr, Equal<Current<TRefNbrField>>,
					  And<ARInvoice.docType, Equal<TDocTypeVal>>>,
				  OrderBy<
					  Asc<ARInvoice.refNbr>>>
					  .SelectSingleBound(Base, currents: new[] { Invoices.Cache.Current });
			return invoice;
		}
	}

	public class SomeGraph : PXGraph<SomeGraph>
	{

	}
}

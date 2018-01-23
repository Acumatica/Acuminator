using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects
{
	public class SOOrderEntry : PXGraph<SOOrderEntry>
	{
		public PXSelectJoin<
			APAdjust,
			InnerJoin<APPayment, On<APPayment.docType, Equal<APAdjust.adjgDocType>, And<APPayment.refNbr, Equal<APAdjust.adjgRefNbr>>>,
			InnerJoin<CurrencyInfo, On<CurrencyInfo.curyInfoID, Equal<APPayment.curyInfoID>>>>,
			Where<APAdjust.adjdDocType, Equal<Current<APInvoice.docType>>, And<APAdjust.adjdRefNbr, Equal<Current<APInvoice.refNbr>>>>>
			Adjustments;
	}
}

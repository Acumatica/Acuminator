using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.AR
{
	public class ARDocumentRelease : PXGraph<ARDocumentRelease>
	{
		[PXFilterable]
		[PX.SM.PXViewDetailsButton(typeof (BalancedARDocument.refNbr), WindowMode = PXRedirectHelper.WindowMode.NewWindow)]
		public PXProcessingJoin<
			BalancedARDocument,
			LeftJoin<ARInvoice, On<ARInvoice.docType, Equal<BalancedARDocument.docType>,
				And<ARInvoice.refNbr, Equal<BalancedARDocument.refNbr>>>,
			LeftJoin<ARPayment, On<ARPayment.docType, Equal<BalancedARDocument.docType>,
				And<ARPayment.refNbr, Equal<BalancedARDocument.refNbr>>>,
			InnerJoinSingleTable<Customer, On<Customer.bAccountID, Equal<BalancedARDocument.customerID>>,
			LeftJoin<ARAdjust, On<ARAdjust.adjgDocType, Equal<BalancedARDocument.docType>,
				And<ARAdjust.adjgRefNbr, Equal<BalancedARDocument.refNbr>,
				And<ARAdjust.adjNbr, Equal<BalancedARDocument.adjCntr>,
				And<ARAdjust.hold, Equal<boolFalse>>>>>>>>>,
			Where2<Match<Customer, Current<AccessInfo.userName>>, 
				And<ARRegister.hold, Equal<boolFalse>, 
				And<ARRegister.voided, Equal<boolFalse>, 
				And<ARRegister.scheduled, Equal<boolFalse>, 
				And<
					Where<BalancedARDocument.released, Equal<boolFalse>,
					And<BalancedARDocument.origModule, Equal<GL.BatchModule.moduleAR>,
					Or<BalancedARDocument.openDoc, Equal<boolTrue>, 
					And<ARAdjust.adjdRefNbr, IsNotNull>>>>>>>>>> 
			ARDocumentList;
	}
}

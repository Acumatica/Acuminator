using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class ReportTaxProcess : PXGraph<ReportTaxProcess>
	{
		public PXSelect<TaxTran> Orders;

		public virtual void VoidReportProc(TaxTran taxTran)
		{
			using (new PXConnectionScope())
			{
				using (PXTransactionScope ts = new PXTransactionScope())
				{
					PXUpdate<
						Set<TaxTran.taxPeriodID, Null,
							Set<TaxTran.revisionID, Null>>,
						TaxTran,
						Where<TaxTran.vendorID, Equal<Required<TaxTran.vendorID>>,
							And<TaxTran.taxPeriodID, Equal<Required<TaxTran.taxPeriodID>>,
							And<TaxTran.revisionID, Equal<Required<TaxTran.revisionID>>,
							And<TaxTran.released, Equal<True>,
							And<TaxTran.voided, Equal<False>>>>>>>
						.Update(this, taxTran.VendorID, taxTran.TaxPeriodID);

					ts.Complete(this);
				}
			}
		}
	}
}

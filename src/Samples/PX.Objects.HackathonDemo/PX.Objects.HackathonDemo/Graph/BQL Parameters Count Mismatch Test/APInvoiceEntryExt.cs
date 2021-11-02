using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PX.Data;
using PX.Objects.AP;

namespace PX.Objects.HackathonDemo.BqlParamsMismatch
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APInvoiceEntryExt : PXGraphExtension<APInvoiceEntry>
	{
		protected virtual void __(Events.RowSelecting<APInvoice> e)
		{
			if (e.Row != null)
			{
				PXSelectBase<APTran> cmd = 
					new PXSelectReadonly<APTran,
								   Where<APTran.tranType, Equal<Required<APTran.tranType>>,
									 And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
									//Accidentally left this And here when WhereAnd was written.
									And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>>(Base);


				if (cmd is PXSelectReadonly<APTran,
								   Where<APTran.tranType, Equal<Required<APTran.tranType>>,
									 And<APTran.refNbr, Equal<Required<APTran.refNbr>>,
									//Accidentally left this And here when WhereAnd was written.
									And<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>>> s)
				{

				}

				

				using (new PXConnectionScope())
				{
					bool b = false;

					//Warning doesn't show in either case.
					if (b)
					{
						cmd.WhereAnd<Where<APTran.lineNbr, Equal<Required<APTran.lineNbr>>>>();

						cmd.Select(e.Row, e.Row.DocType);
					}
					else if (false)
						cmd.Select(e.Row.DocType, e.Row.RefNbr);
				}
			}
		}
	}
}

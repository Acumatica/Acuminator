using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;

namespace PX.Objects.SO
{
	public class BadBqlGraphCopy : PXGraph<BadBqlGraphCopy>
	{
		public PXSelectJoin<BCBudgetBalance, LeftJoin<BCBudgetLedger, On<BCBudgetBalance.ledgerID, Equal<BCBudgetLedger.ledgerID>>>,
			   Where<BCBudgetLedger.summarize, Equal<True>, And<BCBudgetBalance.subledgerID, Equal<Required<BCBudgetBalance.subledgerID>>>>,
			   OrderBy<Asc<BCBudgetBalance.ytdAmount, Asc<BCBudgetBalance.finPeriodID, Asc<BCBudgetBalance.amount>>>>>
		bqlSelect;

		public void Select()
		{
			bool summarize = false;
			int subledgerID = 9;

			var query = PXSelect<BCBudgetBalance, Where<BCBudgetLedger.summarize, Equal<Required<BCBudgetLedger.summarize>>,
				And<BCBudgetBalance.subledgerID, Equal<Required<BCBudgetBalance.subledgerID>>>>,
			OrderBy<Asc<BCBudgetBalance.ytdAmount, Asc<BCBudgetBalance.finPeriodID, Asc<BCBudgetBalance.amount>>>>>.Select(this, summarize, false);
		}
	}
}

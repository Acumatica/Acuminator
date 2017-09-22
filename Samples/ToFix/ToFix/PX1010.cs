using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Api;
using PX.Data;
using PX.Data.Process;
using Action = System.Action;
using PX.SM;

namespace PX1010
{
    public class AUAuditInquire : PXGraph<AUAuditInquire>
    {
        public PXSelect<AUAuditKeys> WE;
        public IEnumerable we()
        {
            String tablename = "we";

            PXSelectBase<AuditHistory> select = new PXSelectGroupBy<AuditHistory, Where<AuditHistory.tableName, Equal<Required<AuditHistory.tableName>>>,
                Aggregate<GroupBy<AuditHistory.combinedKey>>, OrderBy<Asc<AuditHistory.batchID>>>(this);
            select.WhereAnd<Where<AuditHistory.userID, Equal<Current<AUAuditFilter.userID>>>>();
            select.WhereAnd<Where<AuditHistory.changeDate, GreaterEqual<Current<AUAuditFilter.startDate>>>>();
            select.WhereAnd<Where<AuditHistory.changeDate, LessEqual<Current<AUAuditFilter.endDate>>>>();

            int totalRows = 0;
            bool hasSorts = PXView.SortColumns.Length > 1;
            bool hasFilters = PXView.Filters.Length > 0;
            Int32 maxRows = (hasSorts || hasFilters) ? 0 : PXView.MaximumRows;
            Int32 startRow = PXView.StartRow;
            if (maxRows == 1)
            {
                if (PXView.Searches.Length > 0 && PXView.Searches[0] != null)
                    startRow = (Int32)PXView.Searches[0] - 1;
            }
            else if (startRow != 0)
            {
                PXView.StartRow = 0;
            }
            int counter = startRow;

            select.View.Clear();
            foreach (AuditHistory record in select.View.Select(PXView.Currents, new Object[] { tablename },
                null/*PXView.Searches*/, null/*PXView.SortColumns*/, null/*PXView.Descendings*/, PXView.Filters, ref startRow, maxRows, ref totalRows))
            {
                counter++;
                if (String.IsNullOrEmpty(record.CombinedKey)) continue;

                String[] parts = record.CombinedKey.Split(PXAuditHelper.SEPARATOR);
            }
            yield return null;
        }
    }
}
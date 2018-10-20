using PX.Data;
using System.Collections;

namespace PX.Objects
{
    public class SOInvoiceEntryExt2 : PXGraphExtension<SOInvoiceEntryExt, SOInvoiceEntry>
    {
        public IEnumerable invoices2()
        {
            return ExternalMethod2();
        }

        public IEnumerable ExternalMethod2()
        {
            return Base.Invoices2.Select();
        }
    }

    public class SOInvoiceEntryExt : PXGraphExtension<SOInvoiceEntry>
    {
        public IEnumerable invoices()
        {
            var startRow = PXView.StartRow;
            var totalRows = 0;

            var result = ExternalMethod(startRow, totalRows);

            PXView.StartRow = 0;

            return result;
        }

        public IEnumerable ExternalMethod(int startRow, int totalRows)
        {
            return Base.Invoices.View.Select(PXView.Currents, PXView.Parameters, PXView.Searches, PXView.SortColumns,
                PXView.Descendings, PXView.Filters, ref startRow, PXView.MaximumRows, ref totalRows);
        }
    }

    public class SOInvoiceEntry : PXGraph<SOInvoiceEntry, SOInvoice>
    {
        public PXSelect<SOInvoice> Invoices;
        public PXSelect<SOInvoice> Invoices2;
    }

    public class SOInvoice : IBqlTable
    {
        [PXDBString(8, IsKey = true, InputMask = "")]
        public string RefNbr { get; set; }
        public abstract class refNbr : IBqlField { }
    }
}
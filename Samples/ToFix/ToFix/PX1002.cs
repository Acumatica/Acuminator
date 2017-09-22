using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PX.Data;

namespace PX1002
{
    class fml : IBqlTable
    {
        #region InvoicePrecision
        public abstract class invoicePrecision : PX.Data.IBqlField
        {
        }
        protected decimal? _InvoicePrecision;

        [PXDecimalList(new string[] { "0.05", "0.1", "0.5", "1.0", "10", "100" }, new string[] { "0.05", "0.1", "0.5", "1.0", "10", "100" })]
        [PXDefault(TypeCode.Decimal, "0.1")]
        [PXUIField(DisplayName = "Rounding Precision")]
        public virtual decimal? InvoicePrecision
        {
            get
            {
                return this._InvoicePrecision;
            }
            set
            {
                this._InvoicePrecision = value;
            }
        }
        #endregion
    }
}
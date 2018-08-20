using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{ 
	public class POLineExt : PXCacheExtension<POLine>
	{
        #region Cons
        public POLineExt() { }
        #endregion

        #region Cons
        public POLineExt(decimal? total) { Total = total; }
        #endregion


        #region Total
        public abstract class total : IBqlField { }
		[PXDBDecimal]
		[PXUIField(DisplayName = "Total")]
		public decimal? Total { get; set; }
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class TaxTran : IBqlTable
	{
		#region TaxPeriodID
		public abstract class taxPeriodID : PX.Data.IBqlField
		{
		}
		/// <summary>
		/// The key of the tax period to which the record has been reported.
		/// The field has the null value for the unreported records.
		/// </summary>
		[GL.FinPeriodID()]
		[PXDBDefault(typeof(TaxAdjustment.taxPeriod))]
		public virtual String TaxPeriodID
		{
			get
			{
				return this._TaxPeriodID;
			}
			set
			{
				this._TaxPeriodID = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField
		{
		}

		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}

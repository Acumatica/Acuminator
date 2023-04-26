using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;
using PX.Objects.PO;

namespace PX.Objects.HackathonDemo
{
	[PXHidden]
	public class DacFieldWithINUnitAttribute : IBqlTable
	{
		#region UOM
		public abstract class uOM : BqlString.Field<uOM> { }

		[INUnit(typeof(POLineR.inventoryID), DisplayName = "UOM", BqlField = typeof(POLine.uOM))]	//Should not show
		public virtual string UOM { get; set; }
		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.NonNullableTypeForBqlField.Sources
{
	public class SomeDocument : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr : IBqlField { }

		[PXDBInt]
		public int LineNbr { get; set; }
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class SomeDocument : IBqlTable
	{
		#region LineNbr
		public abstract class lineNbr { }

		[PXDBInt]
		public int LineNbr { get; set; }
		#endregion
	}
}

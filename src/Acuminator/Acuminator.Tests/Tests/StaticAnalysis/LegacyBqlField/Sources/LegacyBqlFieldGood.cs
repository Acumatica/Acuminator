using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class GoodDac : IBqlTable
	{
		public abstract class modernField : PX.Data.BQL.BqlString.Field<modernField> { }
		[PXString]
		public string ModernField { get; set; }
	}
}
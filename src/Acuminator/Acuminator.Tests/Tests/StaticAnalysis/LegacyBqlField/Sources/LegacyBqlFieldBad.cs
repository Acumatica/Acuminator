using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class BadDac : IBqlTable
	{
		public abstract class legacyField : IBqlField { }
		[PXString]
		public string LegacyField { get; set; }
	}
}
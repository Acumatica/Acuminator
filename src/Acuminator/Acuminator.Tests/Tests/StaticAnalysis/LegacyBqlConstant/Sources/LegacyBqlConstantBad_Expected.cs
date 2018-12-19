using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class LegacyConst : PX.Data.BQL.BqlString.Constant<LegacyConst> {
		public LegacyConst() : base("") { }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class ModernConst : PX.Data.BQL.BqlString.Constant<ModernConst> {
		public ModernConst() : base("") { }
	}
}
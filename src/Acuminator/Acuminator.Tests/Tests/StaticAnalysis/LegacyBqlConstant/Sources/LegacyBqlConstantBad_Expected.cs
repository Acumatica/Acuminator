using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class LegacyBoolConst : PX.Data.BQL.BqlBool.Constant<LegacyBoolConst> {
		public LegacyBoolConst() : base(false) { }
	}

	public class LegacyByteConst : PX.Data.BQL.BqlByte.Constant<LegacyByteConst> {
		public LegacyByteConst() : base(0) { }
	}

	public class LegacyShortConst : PX.Data.BQL.BqlShort.Constant<LegacyShortConst> {
		public LegacyShortConst() : base(0) { }
	}

	public class LegacyIntConst : PX.Data.BQL.BqlInt.Constant<LegacyIntConst> {
		public LegacyIntConst() : base(0) { }
	}

	public class LegacyLongConst : PX.Data.BQL.BqlLong.Constant<LegacyLongConst> {
		public LegacyLongConst() : base(0) { }
	}

	public class LegacyFloatConst : PX.Data.BQL.BqlFloat.Constant<LegacyFloatConst> {
		public LegacyFloatConst() : base(0) { }
	}

	public class LegacyDoubleConst : PX.Data.BQL.BqlDouble.Constant<LegacyDoubleConst> {
		public LegacyDoubleConst() : base(0) { }
	}

	public class LegacyDecimalConst : PX.Data.BQL.BqlDecimal.Constant<LegacyDecimalConst> {
		public LegacyDecimalConst() : base(0) { }
	}

	public class LegacyStringConst : PX.Data.BQL.BqlString.Constant<LegacyStringConst> {
		public LegacyStringConst() : base("") { }
	}

	public class LegacyDateConst : PX.Data.BQL.BqlDate.Constant<LegacyDateConst> {
		public LegacyDateConst() : base(new DateTime()) { }
	}

	public class LegacyGuidConst : PX.Data.BQL.BqlGuid.Constant<LegacyGuidConst> {
		public LegacyGuidConst() : base(new Guid()) { }
	}
}
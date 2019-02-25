using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class LegacyBoolConst : Constant<Boolean> {
		public LegacyBoolConst() : base(false) { }
	}

	public class LegacyByteConst : Constant<byte> {
		public LegacyByteConst() : base(0) { }
	}

	public class LegacyShortConst : Constant<Int16> {
		public LegacyShortConst() : base(0) { }
	}

	public class LegacyIntConst : Constant<int> {
		public LegacyIntConst() : base(0) { }
	}

	public class LegacyLongConst : Constant<Int64> {
		public LegacyLongConst() : base(0) { }
	}

	public class LegacyFloatConst : Constant<float> {
		public LegacyFloatConst() : base(0) { }
	}

	public class LegacyDoubleConst : Constant<Double> {
		public LegacyDoubleConst() : base(0) { }
	}

	public class LegacyDecimalConst : Constant<decimal> {
		public LegacyDecimalConst() : base(0) { }
	}

	public class LegacyStringConst : Constant<string> {
		public LegacyStringConst() : base("") { }
	}

	public class LegacyDateConst : Constant<DateTime> {
		public LegacyDateConst() : base(new DateTime()) { }
	}

	public class LegacyGuidConst : Constant<Guid> {
		public LegacyGuidConst() : base(new Guid()) { }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	public class ModernBoolConst : PX.Data.BQL.BqlBool.Constant<ModernBoolConst> {
		public ModernBoolConst() : base(false) { }
	}

	public class ModernByteConst : PX.Data.BQL.BqlByte.Constant<ModernByteConst> {
		public ModernByteConst() : base(0) { }
	}

	public class ModernShortConst : PX.Data.BQL.BqlShort.Constant<ModernShortConst> {
		public ModernShortConst() : base(0) { }
	}

	public class ModernIntConst : PX.Data.BQL.BqlInt.Constant<ModernIntConst> {
		public ModernIntConst() : base(0) { }
	}

	public class ModernLongConst : PX.Data.BQL.BqlLong.Constant<ModernLongConst> {
		public ModernLongConst() : base(0) { }
	}

	public class ModernFloatConst : PX.Data.BQL.BqlFloat.Constant<ModernFloatConst> {
		public ModernFloatConst() : base(0) { }
	}

	public class ModernDoubleConst : PX.Data.BQL.BqlDouble.Constant<ModernDoubleConst> {
		public ModernDoubleConst() : base(0) { }
	}

	public class ModernDecimalConst : PX.Data.BQL.BqlDecimal.Constant<ModernDecimalConst> {
		public ModernDecimalConst() : base(0) { }
	}

	public class ModernStringConst : PX.Data.BQL.BqlString.Constant<ModernStringConst> {
		public ModernStringConst() : base("") { }
	}

	public class ModernDateConst : PX.Data.BQL.BqlDate.Constant<ModernDateConst> {
		public ModernDateConst() : base(new DateTime()) { }
	}

	public class ModernGuidConst : PX.Data.BQL.BqlGuid.Constant<ModernGuidConst> {
		public ModernGuidConst() : base(new Guid()) { }
	}

	public class UnsupportedConst1 : Constant<string[]> {
		public UnsupportedConst1() : base(new string[0]) { }
	}

	public class UnsupportedConst2 : Constant<uint> {
		public UnsupportedConst2() : base(0) { }
	}

	public class UnsupportedConst3 : Constant<byte[]> {
		public UnsupportedConst3() : base(new byte[0]) { }
	}
}
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
		public abstract class legacyBoolField : IBqlField { }
		[PXBool]
		public bool? LegacyBoolField { get; set; }

		public abstract class legacyByteField : IBqlField { }
		[PXByte]
		public Byte? LegacyByteField { get; set; }

		public abstract class legacyShortField : IBqlField { }
		[PXShort]
		public short? LegacyShortField { get; set; }

		public abstract class legacyIntField : IBqlField { }
		[PXInt]
		public Int32? LegacyIntField { get; set; }

		public abstract class legacyLongField : IBqlField { }
		[PXLong]
		public long? LegacyLongField { get; set; }

		public abstract class legacyFloatField : IBqlField { }
		[PXFloat]
		public Single? LegacyFloatField { get; set; }

		public abstract class legacyDoubleField : IBqlField { }
		[PXDouble]
		public double? LegacyDoubleField { get; set; }

		public abstract class legacyDecimalField : IBqlField { }
		[PXDecimal]
		public Decimal? LegacyDecimalField { get; set; }

		public abstract class legacyStringField : IBqlField { }
		[PXString]
		public string LegacyStringField { get; set; }

		public abstract class legacyDateField : IBqlField { }
		[PXDate]
		public DateTime? LegacyDateField { get; set; }

		public abstract class legacyGuidField : IBqlField { }
		[PXGuid]
		public Guid? LegacyGuidField { get; set; }

		public abstract class legacyBinaryField : IBqlField { }
		[PXDBBinary]
		public byte[] LegacyBinaryField { get; set; }
	}
}
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
		public abstract class legacyBoolField : PX.Data.BQL.BqlBool.Field<legacyBoolField> { }
		[PXBool]
		public bool? LegacyBoolField { get; set; }

		public abstract class legacyByteField : PX.Data.BQL.BqlByte.Field<legacyByteField> { }
		[PXByte]
		public Byte? LegacyByteField { get; set; }

		public abstract class legacyShortField : PX.Data.BQL.BqlShort.Field<legacyShortField> { }
		[PXShort]
		public short? LegacyShortField { get; set; }

		public abstract class legacyIntField : PX.Data.BQL.BqlInt.Field<legacyIntField> { }
		[PXInt]
		public Int32? LegacyIntField { get; set; }

		public abstract class legacyLongField : PX.Data.BQL.BqlLong.Field<legacyLongField> { }
		[PXLong]
		public long? LegacyLongField { get; set; }

		public abstract class legacyFloatField : PX.Data.BQL.BqlFloat.Field<legacyFloatField> { }
		[PXFloat]
		public Single? LegacyFloatField { get; set; }

		public abstract class legacyDoubleField : PX.Data.BQL.BqlDouble.Field<legacyDoubleField> { }
		[PXDouble]
		public double? LegacyDoubleField { get; set; }

		public abstract class legacyDecimalField : PX.Data.BQL.BqlDecimal.Field<legacyDecimalField> { }
		[PXDecimal]
		public Decimal? LegacyDecimalField { get; set; }

		public abstract class legacyStringField : PX.Data.BQL.BqlString.Field<legacyStringField> { }
		[PXString]
		public string LegacyStringField { get; set; }

		public abstract class legacyDateField : PX.Data.BQL.BqlDateTime.Field<legacyDateField> { }
		[PXDate]
		public DateTime? LegacyDateField { get; set; }

		public abstract class legacyGuidField : PX.Data.BQL.BqlGuid.Field<legacyGuidField> { }
		[PXGuid]
		public Guid? LegacyGuidField { get; set; }

		public abstract class legacyBinaryField : PX.Data.BQL.BqlByteArray.Field<legacyBinaryField> { }
		[PXDBBinary]
		public byte[] LegacyBinaryField { get; set; }
	}
}
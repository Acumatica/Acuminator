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
		public abstract class modernBoolField : PX.Data.BQL.BqlBool.Field<modernBoolField> { }
		[PXBool]
		public bool? ModernBoolField { get; set; }

		public abstract class modernByteField : PX.Data.BQL.BqlByte.Field<modernByteField> { }
		[PXByte]
		public byte? ModernByteField { get; set; }

		public abstract class modernShortField : PX.Data.BQL.BqlShort.Field<modernShortField> { }
		[PXShort]
		public short? ModernShortField { get; set; }

		public abstract class modernIntField : PX.Data.BQL.BqlInt.Field<modernIntField> { }
		[PXInt]
		public int? ModernIntField { get; set; }

		public abstract class modernLongField : PX.Data.BQL.BqlLong.Field<modernLongField> { }
		[PXLong]
		public long? ModernLongField { get; set; }

		public abstract class modernFloatField : PX.Data.BQL.BqlFloat.Field<modernFloatField> { }
		[PXFloat]
		public float? ModernFloatField { get; set; }

		public abstract class modernDoubleField : PX.Data.BQL.BqlDouble.Field<modernDoubleField> { }
		[PXDouble]
		public double? ModernDoubleField { get; set; }

		public abstract class modernDecimalField : PX.Data.BQL.BqlDecimal.Field<modernDecimalField> { }
		[PXDecimal]
		public decimal? ModernDecimalField { get; set; }

		public abstract class modernStringField : PX.Data.BQL.BqlString.Field<modernStringField> { }
		[PXString]
		public String ModernStringField { get; set; }

		public abstract class modernDateField : PX.Data.BQL.BqlDate.Field<modernDateField> { }
		[PXDate]
		public DateTime? ModernDateField { get; set; }

		public abstract class modernGuidField : PX.Data.BQL.BqlGuid.Field<modernGuidField> { }
		[PXGuid]
		public Guid? ModernGuidField { get; set; }

		public abstract class modernBinaryField : PX.Data.BQL.BqlByteArray.Field<modernBinaryField> { }
		[PXDBBinary]
		public byte[] ModernBinaryField { get; set; }

		public abstract class unsupportedField1 : IBqlField { }
		public uint UnsupportedField1 { get; set; }

		public abstract class unsupportedField2 : IBqlField { }
		public uint[] UnsupportedField2 { get; set; }

		public abstract class unsupportedField3 : IBqlField { }
		public string[] UnsupportedField3 { get; set; }
	}
}
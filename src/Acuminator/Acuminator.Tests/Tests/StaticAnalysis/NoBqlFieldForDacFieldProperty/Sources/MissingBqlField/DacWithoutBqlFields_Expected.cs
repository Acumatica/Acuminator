using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithoutBqlFields : IBqlTable
	{
		public abstract class boolField : PX.Data.BQL.BqlBool.Field<boolField> { }

		[PXDBBool]
		public bool? BoolField { get; set; }

		public abstract class boolField2 : PX.Data.BQL.BqlBool.Field<boolField2> { }

		[PXDBBool]
		public Boolean? BoolField2 { get; set; }

		public abstract class byteField : PX.Data.BQL.BqlByte.Field<byteField> { }

		[PXDBByte]
		public Byte? ByteField { get; set; }

		public abstract class byteField2 : PX.Data.BQL.BqlByte.Field<byteField2> { }

		[PXDBByte]
		public byte? ByteField2 { get; set; }

		public abstract class shortField : PX.Data.BQL.BqlShort.Field<shortField> { }

		[PXShort]
		public short? ShortField { get; set; }

		public abstract class shortField2 : PX.Data.BQL.BqlShort.Field<shortField2> { }

		[PXShort]
		public Int16? ShortField2 { get; set; }

		public abstract class intField : PX.Data.BQL.BqlInt.Field<intField> { }

		[PXInt]
		public Int32? IntField { get; set; }

		public abstract class intField2 : PX.Data.BQL.BqlInt.Field<intField2> { }

		[PXInt]
		public int? IntField2 { get; set; }

		public abstract class longField : PX.Data.BQL.BqlLong.Field<longField> { }

		[PXLong]
		public long? LongField { get; set; }

		public abstract class longField2 : PX.Data.BQL.BqlLong.Field<longField2> { }

		[PXLong]
		public Int64? LongField2 { get; set; }

		public abstract class floatField : PX.Data.BQL.BqlFloat.Field<floatField> { }

		[PXFloat]
		public Single? FloatField { get; set; }

		public abstract class floatField2 : PX.Data.BQL.BqlFloat.Field<floatField2> { }

		[PXFloat]
		public float? FloatField2 { get; set; }

		public abstract class doubleField : PX.Data.BQL.BqlDouble.Field<doubleField> { }

		[PXDouble]
		public double? DoubleField { get; set; }

		public abstract class doubleField2 : PX.Data.BQL.BqlDouble.Field<doubleField2> { }

		[PXDouble]
		public Double? DoubleField2 { get; set; }

		public abstract class decimalField : PX.Data.BQL.BqlDecimal.Field<decimalField> { }

		[PXDecimal]
		public Decimal? DecimalField { get; set; }
		#region DecimalField2
		public abstract class decimalField2 : PX.Data.BQL.BqlDecimal.Field<decimalField2> { }

		[PXDecimal]
		public decimal? DecimalField2 { get; set; }
		#endregion
		public abstract class stringField : PX.Data.BQL.BqlString.Field<stringField> { }
		[PXString]
		public string StringField { get; set; }
		#region StringField2
		public abstract class stringField2 : PX.Data.BQL.BqlString.Field<stringField2> { }

		[PXString]
		public String StringField2 { get; set; }
		#endregion
		#region DateField
		public abstract class dateField : PX.Data.BQL.BqlDateTime.Field<dateField> { }
		[PXDate]
		public DateTime? DateField { get; set; }
		#endregion
		public abstract class guidField : PX.Data.BQL.BqlGuid.Field<guidField> { }

		[PXGuid]
		public Guid? GuidField { get; set; }

		public abstract class binaryField : PX.Data.BQL.BqlByteArray.Field<binaryField> { }

		[PXDBBinary]
		public byte[] BinaryField { get; set; }
		#region BinaryField2
		public abstract class binaryField2 : PX.Data.BQL.BqlByteArray.Field<binaryField2> { }

		[PXDBBinary]
		public Byte[] BinaryField2 { get; set; }
		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasGuidField => GuidField != null;

		public abstract class attributes : PX.Objects.CR.BqlAttributes.Field<attributes> { }

		[PXUIField]
		public virtual string[] Attributes { get; set; }
	}
}
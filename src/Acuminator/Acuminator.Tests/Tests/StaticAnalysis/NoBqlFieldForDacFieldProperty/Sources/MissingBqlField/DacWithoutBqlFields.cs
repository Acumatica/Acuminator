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
		[PXDBBool]
		public bool? BoolField { get; set; }

		[PXDBBool]
		public Boolean? BoolField2 { get; set; }

		[PXDBByte]
		public Byte? ByteField { get; set; }

		[PXDBByte]
		public byte? ByteField2 { get; set; }

		[PXShort]
		public short? ShortField { get; set; }

		[PXShort]
		public Int16? ShortField2 { get; set; }

		[PXInt]
		public Int32? IntField { get; set; }

		[PXInt]
		public int? IntField2 { get; set; }

		[PXLong]
		public long? LongField { get; set; }

		[PXLong]
		public Int64? LongField2 { get; set; }

		[PXFloat]
		public Single? FloatField { get; set; }

		[PXFloat]
		public float? FloatField2 { get; set; }

		[PXDouble]
		public double? DoubleField { get; set; }

		[PXDouble]
		public Double? DoubleField2 { get; set; }

		[PXDecimal]
		public Decimal? DecimalField { get; set; }

		#region DecimalField2
		[PXDecimal]
		public decimal? DecimalField2 { get; set; }
		#endregion
		[PXString]
		public string StringField { get; set; }

		#region StringField2
		[PXString]
		public String StringField2 { get; set; }
		#endregion
		#region DateField
		[PXDate]
		public DateTime? DateField { get; set; }
		#endregion

		[PXGuid]
		public Guid? GuidField { get; set; }

		[PXDBBinary]
		public byte[] BinaryField { get; set; }

		#region BinaryField2
		[PXDBBinary]
		public Byte[] BinaryField2 { get; set; }
		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasGuidField => GuidField != null;
	}
}
using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DerivedDac : BaseDac
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
	}

	[PXHidden]
	public class BaseDac : IBqlTable
	{
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion

		[PXInt]
		public virtual int? ShipmentNbr { get; set; }

		public virtual string ExtraData { get; set; }
	}
}
using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DerivedDac : BaseDac
	{
		public new abstract class statyus : PX.Data.BQL.BqlString.Field<statyus> { }
	}

	[PXHidden]
	public class BaseDac : IBqlTable
	{
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }

		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
		#endregion

		public virtual string ExtraData { get; set; }
	}
}
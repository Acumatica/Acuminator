using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DerivedDac : BaseDac
	{
		
	}

	[PXHidden]
	public class BaseDac : IBqlTable
	{
		#region Status
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }

		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion
	}
}
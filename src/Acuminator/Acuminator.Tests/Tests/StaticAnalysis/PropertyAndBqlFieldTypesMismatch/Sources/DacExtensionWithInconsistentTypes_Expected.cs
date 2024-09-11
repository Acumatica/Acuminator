using System;

using PX.Data;
using PX.Data.BQL;

namespace PX.Analyzers.Test.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public sealed class DacExt : PXCacheExtension<Dac>
	{
		#region LineNbr
		[PXUIField]
		public int? LineNbr { get; set; }

		public abstract class lineNbr : Data.BQL.BqlInt.Field<lineNbr> { }
		#endregion

		#region OrderType
		public abstract class orderType : BqlString.Field<orderType> { }

		[PXUIField]
		public string OrderType { get; set; }
		#endregion

		[PXUIField]
		public Guid? NoteID { get; set; }

		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
	}

	[PXHidden]
	public class Dac : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion

		#region tstamp
		public abstract class Tstamp : IBqlField { }

		[PXUIField]
		public virtual byte[] tstamp { get; set; }
		#endregion
	}
}
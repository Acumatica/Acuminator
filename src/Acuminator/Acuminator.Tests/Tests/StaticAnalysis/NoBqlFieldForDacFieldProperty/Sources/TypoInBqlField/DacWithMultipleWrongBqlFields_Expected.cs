using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithMultipleWrongBqlFields : IBqlTable
	{
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		public Guid? NoteID { get; set; }

		public abstract class noteID1 : PX.Data.BQL.BqlGuid.Field<noteID1> { }

		public abstract class noteLD : PX.Data.BQL.BqlGuid.Field<noteLD> { }

		[PXGuid]
		public Guid? NoteID1 { get; set; }

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasNoteID => NoteID != null;
	}
}
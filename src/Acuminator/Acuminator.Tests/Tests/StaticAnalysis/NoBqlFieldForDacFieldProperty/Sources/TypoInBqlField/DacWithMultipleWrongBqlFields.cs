using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithMultipleWrongBqlFields : IBqlTable
	{
		public abstract class n0teID : PX.Data.BQL.BqlGuid.Field<n0teID> { }

		[PXGuid]
		public Guid? NoteID { get; set; }

		public abstract class nteID : PX.Data.BQL.BqlGuid.Field<nteID> { }

		public abstract class noteLD : PX.Data.BQL.BqlGuid.Field<noteLD> { }

		[PXGuid]
		public Guid? NoteID1 { get; set; }

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasNoteID => NoteID != null;
	}
}
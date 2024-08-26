using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithSingleWrongBqlFields : IBqlTable
	{
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXGuid]
		public Guid? NoteID { get; set; }

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasNoteID => NoteID != null;
	}
}
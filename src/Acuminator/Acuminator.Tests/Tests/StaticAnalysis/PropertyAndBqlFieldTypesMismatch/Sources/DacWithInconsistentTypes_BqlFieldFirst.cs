using System;

using PX.Data;
using PX.Data.BQL;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithBqlFieldsFirst : IBqlTable
	{
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlInt.Field<noteID> { }

		[PXGuid]
		public Guid? NoteID { get; set; }
		#endregion

		#region tstamp
		public abstract class Tstamp : BqlByte.Field<Tstamp> { }

		[PXDBTimestamp(RecordComesFirst = true)]
		public virtual byte[] tstamp { get; set; }
		#endregion

		#region Shipment Nbr
		public abstract class shipmentNbr : IBqlField { }	// no strong BQL type => shouldn't be reported

		[PXInt]
		public virtual int? ShipmentNbr { get; set; }
		#endregion

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public bool HasNoteID => NoteID != null;
	}
}
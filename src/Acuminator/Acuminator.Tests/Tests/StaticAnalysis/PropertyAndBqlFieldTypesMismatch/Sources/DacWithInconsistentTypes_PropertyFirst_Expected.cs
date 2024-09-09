using System;

using PX.Data;
using PX.Data.BQL;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DacWithPropertiesFirst : IBqlTable
	{
		#region NoteID
		[PXUIField]
		public Guid? NoteID { get; set; }

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		#endregion

		#region tstamp
		[PXUIField]
		public virtual byte[] tstamp { get; set; }

		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		#endregion

		#region OrderType
		[PXUIField]
		public virtual string OrderType { get; set; }

		public abstract class orderType : Data.BQL.BqlString.Field<orderType> { }
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
using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	public sealed class DacExtension : PXCacheExtension<BaseDac>
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }

		#region Selected
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion
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
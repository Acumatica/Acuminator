#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace ExternalDependency.NoBqlFieldForDacFieldProperty
{
	[PXHidden]
	public class BaseDacWithoutBqlField : IBqlTable
    {
		[PXHidden]
		public class BaseDac : IBqlTable
		{
			#region Status
			public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
			#endregion

			[PXInt]
			public virtual int? ShipmentNbr { get; set; }

			public virtual string? ExtraData { get; set; }
		}
	}
}

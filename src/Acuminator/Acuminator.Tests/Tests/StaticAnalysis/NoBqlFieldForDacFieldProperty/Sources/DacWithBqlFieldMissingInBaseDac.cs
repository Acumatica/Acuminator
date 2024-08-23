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
		[PXInt]
		public virtual int? ShipmentNbr { get; set; }
	}
}
using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	[PXHidden]
	public class DerivedDac : ExternalDependency.NoBqlFieldForDacFieldProperty.BaseDacWithoutBqlField
	{
		[PXString]
		[PXUIField(DisplayName = "Order Number")]
		public virtual string OrderNbr { get; set; }

		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
	}
}
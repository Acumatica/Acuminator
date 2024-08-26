using System;

using PX.Data;

using ExternalDependency.NoBqlFieldForDacFieldProperty;

namespace PX.Analyzers.Test.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	public sealed class DacExtensionOnExternalDac : PXCacheExtension<BaseDacWithoutBqlField>
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string Status { get; set; }
	}
}
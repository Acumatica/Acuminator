using System;
using System.Collections.Generic;
using System.Linq;



namespace Acuminator.Vsix.ChangesClassification
{
	/// <summary>
	/// Values that represent the location of changes in the document.
	/// </summary>
	[Flags]
	public enum ChangeLocation
	{
		None      = 0b0000,
		Method    = 0b0001,
		Property  = 0b0010,
		Class     = 0b0100,
		Namespace = 0b1000
	}
}

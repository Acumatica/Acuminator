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
		None = 0b0000_0000,
		/// <summary>
		/// The change is made inside a block of statements - a body of a method, property get/set, event add/remove or in type's field storing delegate with field initializer setting lambda.
		/// </summary>
		StatementsBlock = 0b0000_0001,
		Attributes = 0b0000_0010,
		Class     = 0b0100_0000,
		Namespace = 0b1000_0000
	}
}

using System;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Values that represent attribute placements.
	/// </summary>
	public enum AttributePlacement : byte
	{
		Dac,
		DacField,
		Graph,
		CacheAttached
	}
}

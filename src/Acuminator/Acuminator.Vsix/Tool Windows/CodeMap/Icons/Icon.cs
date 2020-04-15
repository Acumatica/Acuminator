using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Values that represent icons for tree nodes in code map tree.
	/// </summary>
	public enum Icon : byte
	{
		None,
		Graph,
		GraphEventCategory,
		Dac,

		DacProperty,	
		DacKeyProperty,

		DacPropertiesCategory,
		DacKeysCategory,

		Category,
		GroupingDac,
		GroupingDacField,

		View,
		ViewDelegate,

		Action,
		ActionHandler,

		PXOverride,

		RowEvent,
		FieldEvent,
		CacheAttached,

		Settings,
		Filter,
		Processing,
	}
}

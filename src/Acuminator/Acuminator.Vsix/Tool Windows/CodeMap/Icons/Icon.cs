using System;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Values that represent icons for tree nodes in code map tree.
	/// </summary>
	public enum Icon
	{
		None,
		Graph,
		Event,
		Dac,

		DacProperty,	
		DacKeyProperty,
		DacIdentityProperty,
		DacKeyIdentityProperty,

		DacPropertiesCategory,
		DacKeysCategory,

		GroupNode,
		GroupingDac,
		GroupingDacField,

		View,
		ViewDelegate,

		Action,
		ActionHandler,

		PXOverride,

		RowEvent,
		FieldEvent,
		CacheAttached	
	}
}

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
		GraphExtension,
		GraphEventCategory,
		Dac,
		DacExtension,

		DacField,
		DacKeyField,
		DacFieldProperty,
		DacBqlField,

		DacPropertiesCategory,
		DacKeysCategory,

		Category,
		BaseTypesCategory,

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

		BaseMemberOverrideGraphCategory,
		MethodOverrideGraph,
		PropertyOverrideGraph,
		EventOverrideGraph,
		PersistMethodOverride,

		InitializationAndActivationGraphCategory,
		IsActiveMethodGraph,
		ConfigureMethodGraph,
		InitializeMethodGraph,
		StaticGraphConstructor,
		InstanceGraphConstructor,

		InitializationAndActivationDacCategory,
		IsActiveMethodDac,

		ProjectionDac,

		Attribute,
		AttributesGroup,
		ProjectionAttribute
	}
}

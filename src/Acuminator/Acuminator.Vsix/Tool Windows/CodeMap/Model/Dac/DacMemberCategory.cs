#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Dac
{
	public enum DacMemberCategory
	{
		BaseTypes,
		Keys,
		Property,
		FieldsWithoutProperty,
		InitializationAndActivation
	}

	internal static class DacMemberTypeTypeUtils
	{
		private static readonly Dictionary<DacMemberCategory, string> _descriptions = new Dictionary<DacMemberCategory, string>
		{
			{ DacMemberCategory.BaseTypes, "Base Types" },
			{ DacMemberCategory.Keys, "Keys" },
			{ DacMemberCategory.Property, "DAC Fields" },
			{ DacMemberCategory.FieldsWithoutProperty, "Fields without property" },
			{ DacMemberCategory.InitializationAndActivation, "Initialization & Activation" }
		};

		public static string Description(this DacMemberCategory dacMemberCategory) =>
			_descriptions.TryGetValue(dacMemberCategory, out string description)
				? description
				: string.Empty;
	}
}

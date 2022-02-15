using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public enum DacMemberCategory
	{
		Keys,
		Property,		
		FieldsWithoutProperty,
		InitializationAndActivation
	}

	internal static class DacMemberTypeTypeUtils
	{
		private static readonly Dictionary<DacMemberCategory, string> _descriptions = new Dictionary<DacMemberCategory, string>
		{
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

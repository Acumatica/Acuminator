using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public enum DacMemberType
	{
		Property,
		Field
	}

	internal static class DacMemberTypeTypeUtils
	{
		private static readonly Dictionary<DacMemberType, string> _descriptions = new Dictionary<DacMemberType, string>
		{
			{ DacMemberType.Property, "Properties" },
			{ DacMemberType.Field, "Fields" },
		};

		public static string Description(this DacMemberType dacMemberType) =>
			_descriptions.TryGetValue(dacMemberType, out string description)
				? description
				: string.Empty;
	}
}

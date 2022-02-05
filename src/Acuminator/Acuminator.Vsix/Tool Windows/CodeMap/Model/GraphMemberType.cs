using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public enum GraphMemberType
	{
		View,
		Action,
		CacheAttached,
		RowEvent,
		FieldEvent,
		NestedDAC,
		NestedGraph,
		PXOverride,
		InitializationAndActivation,
		BaseMemberOverride
	}

	internal static class GraphMemberTypeUtils
	{
		private static readonly Dictionary<GraphMemberType, string> _descriptions = new Dictionary<GraphMemberType, string>
		{
			{ GraphMemberType.View, "Views" },
			{ GraphMemberType.Action, "Actions" },
			{ GraphMemberType.PXOverride, "PXOverrides" },
			{ GraphMemberType.CacheAttached, "Cache Attached" },
			{ GraphMemberType.RowEvent, "Row Events" },
			{ GraphMemberType.FieldEvent, "Field Events" },
			{ GraphMemberType.NestedDAC, "Nested DACs" },
			{ GraphMemberType.NestedGraph, "Nested Graphs" },
			{ GraphMemberType.InitializationAndActivation, "Initialization & Activation" },	
			{ GraphMemberType.BaseMemberOverride, "Base Overrides" }	
		};

		public static string Description(this GraphMemberType graphMemberType) =>
			_descriptions.TryGetValue(graphMemberType, out string description)
				? description
				: string.Empty;
	}
}

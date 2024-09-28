using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap.Graph
{
	public enum GraphMemberCategory
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

	internal static class GraphMemberCategoryUtils
	{
		private static readonly Dictionary<GraphMemberCategory, string> _descriptions = new Dictionary<GraphMemberCategory, string>
		{
			{ GraphMemberCategory.View						 , "Views" },
			{ GraphMemberCategory.Action					 , "Actions" },
			{ GraphMemberCategory.PXOverride				 , "PXOverrides" },
			{ GraphMemberCategory.CacheAttached				 , "Cache Attached" },
			{ GraphMemberCategory.RowEvent					 , "Row Events" },
			{ GraphMemberCategory.FieldEvent				 , "Field Events" },
			{ GraphMemberCategory.NestedDAC					 , "Nested DACs" },
			{ GraphMemberCategory.NestedGraph				 , "Nested Graphs" },
			{ GraphMemberCategory.InitializationAndActivation, "Initialization & Activation" },
			{ GraphMemberCategory.BaseMemberOverride		 , "Base Overrides" }
		};

		public static string Description(this GraphMemberCategory graphMemberType) =>
			_descriptions.TryGetValue(graphMemberType, out string description)
				? description
				: string.Empty;
	}
}

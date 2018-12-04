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
		ViewDelegate,
		Action,
		ActionHandler,
		CacheAttached,
		RowEvent,
		FieldEvent,
		NestedDAC,
		NestedGraph
	}

	internal static class GraphMemberTypeUtils
	{
		private static readonly Dictionary<GraphMemberType, string> _descriptions = new Dictionary<GraphMemberType, string>
		{
			{ GraphMemberType.View, "Views" },
			{ GraphMemberType.ViewDelegate, "View Delegates" },
			{ GraphMemberType.Action, "Actions" },
			{ GraphMemberType.ActionHandler, "Action Handlers" },
			{ GraphMemberType.CacheAttached, "Cache Attached" },
			{ GraphMemberType.RowEvent, "Row Events" },
			{ GraphMemberType.FieldEvent, "Field Events" },
			{ GraphMemberType.NestedDAC, "Nested DACs" },
			{ GraphMemberType.NestedGraph, "Nested Graphs" },
		};

		public static string Description(this GraphMemberType graphMemberType) =>
			_descriptions.TryGetValue(graphMemberType, out string description)
				? description
				: string.Empty;

		public static ImmutableArray<GraphMemberType> GetGraphMemberTypes() => _descriptions.Keys.ToImmutableArray();
	}
}

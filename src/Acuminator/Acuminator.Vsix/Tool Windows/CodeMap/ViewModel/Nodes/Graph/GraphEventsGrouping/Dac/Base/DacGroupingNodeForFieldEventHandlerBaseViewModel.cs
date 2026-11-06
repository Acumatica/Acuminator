using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic.AcumaticaEvents;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public abstract class DacGroupingNodeForFieldEventHandlerBaseViewModel<TFieldEventHandlerInfo> : DacGroupingNodeBaseViewModel
	where TFieldEventHandlerInfo : GraphFieldEventHandlerInfoBase<TFieldEventHandlerInfo>
	{
		public ImmutableArray<TFieldEventHandlerInfo> AllFieldEventHandlers { get; }

		public DacGroupingNodeForFieldEventHandlerBaseViewModel(GraphEventHandlerCategoryNodeViewModel graphEventHandlersCategoryVM, string dacName,
																IEnumerable<TFieldEventHandlerInfo> fieldEventHandlers,
																ExpandCreatedNodesFunction isExpandedCalculator) :
															base(graphEventHandlersCategoryVM, dacName, isExpandedCalculator)
		{
			AllFieldEventHandlers = fieldEventHandlers?.ToImmutableArray() ?? ImmutableArray.Create<TFieldEventHandlerInfo>();
		}
	}
}
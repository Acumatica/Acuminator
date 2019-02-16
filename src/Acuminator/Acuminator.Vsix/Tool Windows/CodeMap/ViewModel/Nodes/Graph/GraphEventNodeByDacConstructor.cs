using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	public delegate GraphMemberNodeViewModel GraphEventNodeByDacConstructor(DacGroupingNodeViewModel dacGroup, 
																			GraphEventInfo graphEventInfo,
																			bool isExpanded);
}

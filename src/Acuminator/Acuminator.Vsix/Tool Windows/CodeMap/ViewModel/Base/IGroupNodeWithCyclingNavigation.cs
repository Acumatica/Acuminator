#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Acuminator.Vsix.ToolWindows.CodeMap;

public interface IGroupNodeWithCyclingNavigation
{
	bool AllowNavigation { get; }

	int CurrentNavigationIndex
	{
		get;
		set;
	}

	IList<TreeNodeViewModel> DisplayedChildren { get; }

	bool CanNavigateToChild(TreeNodeViewModel child);
}

#nullable enable

using System;
using System.Collections.Generic;

namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal static class CyclingNavigationUtils
	{
		public static TreeNodeViewModel? GetChildToNavigateTo(this IGroupNodeWithCyclingNavigation groupNode)
		{
			if (groupNode?.AllowNavigation != true || groupNode.DisplayedChildren.Count == 0)
				return null;

			int counter = 0;

			while (counter < groupNode.DisplayedChildren.Count)
			{
				TreeNodeViewModel child = groupNode.DisplayedChildren[groupNode.CurrentNavigationIndex];
				groupNode.CurrentNavigationIndex = (groupNode.CurrentNavigationIndex + 1) % groupNode.DisplayedChildren.Count;

				if (groupNode.CanNavigateToChild(child))
				{			
					return child;
				}

				counter++;
			}

			return null;
		}
	}
}

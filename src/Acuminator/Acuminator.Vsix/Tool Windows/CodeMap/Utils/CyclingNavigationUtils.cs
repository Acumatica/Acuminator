using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	internal static class CyclingNavigationUtils
	{
		public static TreeNodeViewModel GetChildToNavigateTo(this IGroupNodeWithCyclingNavigation groupNode)
		{
			if (groupNode?.AllowNavigation != true || groupNode.Children.Count == 0)
				return null;

			int counter = 0;

			while (counter < groupNode.Children.Count)
			{
				TreeNodeViewModel child = groupNode.Children[groupNode.CurrentNavigationIndex];
				groupNode.CurrentNavigationIndex = (groupNode.CurrentNavigationIndex + 1) % groupNode.Children.Count;

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

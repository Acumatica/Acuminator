using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Acuminator.Utilities.Common;


namespace Acuminator.Vsix.Utilities
{
	/// <summary>
	/// A static helper for the WPF Visual Tree.
	/// </summary>
	internal static class VisualTreeUtils
	{
		/// <summary>
		/// Gets the visual descendants of the <paramref name="uiElement"/> from the visual tree.
		/// </summary>
		/// <param name="uiElement">The UI element to act on.</param>
		/// <returns/>
		public static IEnumerable<FrameworkElement> GetVisualDescendants(this FrameworkElement uiElement)
		{
			uiElement.ThrowOnNull(nameof(uiElement));

			int childrenCount = VisualTreeHelper.GetChildrenCount(uiElement);

			if (childrenCount == 0)
				return Enumerable.Empty<FrameworkElement>();

			return GetVisualDescendantsImpl(uiElement, childrenCount);

			//-------------------------------------------Local function-----------------------------------
			static IEnumerable<FrameworkElement> GetVisualDescendantsImpl(FrameworkElement parent, int childrenCount)
			{
				for (int i = 0; i < childrenCount; i++)
				{
					var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

					if (child == null)
						continue;

					yield return child;

					int grandchildrenCount = VisualTreeHelper.GetChildrenCount(child);

					foreach (var descendant in GetVisualDescendantsImpl(child, grandchildrenCount).OfType<FrameworkElement>())
					{
						yield return descendant;
					}
				}
			}
		}
	}
}

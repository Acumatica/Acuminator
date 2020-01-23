using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Acuminator.Utilities.Common;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.Themes
{
	public partial class Styles : ResourceDictionary
	{
		/// <summary>
		/// A "hack" that removes a strange small white rectangle on load of context menu by searching for responsible rectangles in the visual tree and hidhing them.
		/// </summary>
		/// <param name="sender">Source of the event.</param>
		/// <param name="e">Event information to send to registered event handlers.</param>
		private void OnContextMenuLoaded(object sender, RoutedEventArgs e)
		{
			if (!(sender is ContextMenu contextMenu))
				return;

			contextMenu.GetVisualDescendants()
					   .OfType<Rectangle>()
					   .ForEach(rectangle => rectangle.Visibility = Visibility.Collapsed);
		}
	}
}
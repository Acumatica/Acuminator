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
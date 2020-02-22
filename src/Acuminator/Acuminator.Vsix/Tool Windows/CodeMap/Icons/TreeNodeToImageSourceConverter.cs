using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Converter which converts <see cref="IconViewModel"/> to the <see cref="BitmapImage"/> icon.
	/// </summary>
	[ValueConversion(sourceType: typeof(IconViewModel), targetType: typeof(BitmapImage))]
	public class TreeNodeToImageSourceConverter : IValueConverter
	{
		private const string BitmapsCollectionURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap/Bitmap/BitmapImages.xaml";

		private ResourceDictionary _resourceDictionary = new ResourceDictionary()
		{
			Source = new Uri(BitmapsCollectionURI)
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			string iconKey = null;

			switch (value)
			{
				case IconViewModel iconViewModel when iconViewModel.IconType != Icon.None:
					iconKey = iconViewModel.IconType.ToString();
					break;

				case TreeNodeViewModel nodeViewModel when nodeViewModel.NodeIcon != Icon.None:
					iconKey = nodeViewModel.NodeIcon.ToString();
					break;
			}

			if (iconKey == null)
				return null;

			return _resourceDictionary.TryGetValue(iconKey, out BitmapImage icon)
				? icon
				: null;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}

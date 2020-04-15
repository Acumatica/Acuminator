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
	/// Converter which converts <see cref="ViewModelBase"/> to the <see cref="BitmapImage"/> icon.
	/// </summary>
	[ValueConversion(sourceType: typeof(ViewModelBase), targetType: typeof(BitmapImage))]
	public class TreeNodeToImageSourceConverter : IValueConverter
	{
		private const string BitmapsCollectionURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap/Bitmap/BitmapImages.xaml";
		private const string SmallIconSuffix = "Small";

		private ResourceDictionary _resourceDictionary = new ResourceDictionary()
		{
			Source = new Uri(BitmapsCollectionURI)
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value)
			{
				case IconViewModel iconViewModel when iconViewModel.IconType != Icon.None:
				{
					string iconKey = iconViewModel.IconType.ToString();
					string smallIconKey = iconKey + SmallIconSuffix;

					if (_resourceDictionary.TryGetValue(smallIconKey, out BitmapImage icon) ||
						_resourceDictionary.TryGetValue(iconKey, out icon))
					{
						return icon;
					}

					return null;
				}
				case TreeNodeViewModel nodeViewModel when nodeViewModel.NodeIcon != Icon.None:
				{
					string iconKey = nodeViewModel.NodeIcon.ToString();
					return _resourceDictionary.TryGetValue(iconKey, out BitmapImage icon)
						? icon
						: null;
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}

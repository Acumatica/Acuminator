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
		private const string BitmapsCollectionURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap/Themes/BitmapImages.xaml";

		private ResourceDictionary _resourceDictionary = new ResourceDictionary()
		{
			Source = new Uri(BitmapsCollectionURI)
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is IconViewModel iconViewModel) || iconViewModel.IconType == Icon.None)
				return null;

			string iconKey = iconViewModel.IconType.ToString();
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

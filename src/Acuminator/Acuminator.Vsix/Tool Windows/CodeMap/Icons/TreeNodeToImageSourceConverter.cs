#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using Acuminator.Vsix.Utilities;


namespace Acuminator.Vsix.ToolWindows.CodeMap
{
	/// <summary>
	/// Multi value converter which converts <see cref="ViewModelBase"/> and <see cref="Icon"/> to the <see cref="BitmapImage"/> icon.<br/>
	/// The second <see cref="Icon"/> parameter is not actually used, it is present to allow dynamic refresh of icons on VS theme change.
	/// </summary>
	public class TreeNodeToImageSourceConverter : IMultiValueConverter
	{
		private const string BitmapsCollectionURI = @"pack://application:,,,/Acuminator;component/Resources/CodeMap/Bitmap/BitmapImages.xaml";
		private const string SmallIconSuffix = "Small";
		private const string LightIconSuffix = "Light";
		private const string DarkIconSuffix = "Dark";

		private ResourceDictionary _resourceDictionary = new()
		{
			Source = new Uri(BitmapsCollectionURI)
		};

		public object? Convert(object?[]? values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values?.Length != 2 || values[0] is not ViewModelBase viewModel)
				return null;

			switch (viewModel)
			{
				case IconViewModel iconViewModel when iconViewModel.IconType != Icon.None:
					{
						string iconKey = iconViewModel.IconType.ToString();
						string smallIconKey = iconKey + SmallIconSuffix;

						if (_resourceDictionary.TryGetValue(smallIconKey, out BitmapImage? icon) ||
							_resourceDictionary.TryGetValue(iconKey, out icon))
						{
							return icon;
						}

						return null;
					}
				case TreeNodeViewModel nodeViewModel when nodeViewModel.NodeIcon != Icon.None:
					{
						string iconKey = nodeViewModel.NodeIcon.ToString();

						if (nodeViewModel.IconDependsOnCurrentTheme)
						{
							string themeSuffix = nodeViewModel.Tree.CodeMapViewModel.IsDarkTheme
								? DarkIconSuffix
								: LightIconSuffix;
							iconKey += themeSuffix;
						}

						return _resourceDictionary.TryGetValue(iconKey, out BitmapImage? icon)
							? icon
							: null;
					}
			}

			return null;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => 
			throw new NotSupportedException();
	}
}

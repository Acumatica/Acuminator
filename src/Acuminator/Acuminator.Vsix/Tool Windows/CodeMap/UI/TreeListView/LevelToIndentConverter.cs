using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;


namespace Acuminator.Vsix.ToolWindows.CodeMap.TreeListViewControl
{
    /// <summary>
    /// Convert Level to left margin
    /// </summary>
    [ValueConversion(sourceType: typeof(int), targetType: typeof(Thickness))]
    public class LevelToIndentConverter : IValueConverter
    {
        private const double IndentSize = 19.0;

        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            if (!(o is int level))
                return Binding.DoNothing;

            return new Thickness(level * IndentSize, 0, 0, 0);
        }

        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture) => throw new NotSupportedException();        
    }
}

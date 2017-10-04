using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ronin.Utilities
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToBrushConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            bool state = (bool)value;

            FrameworkElement FrameElem = new FrameworkElement();

            if (state == true)
                return Brushes.DarkRed;
            else
                return Brushes.DarkGreen;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

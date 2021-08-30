using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LoadProfileGenerator.Controls.Converters
{
    /// <summary>
    /// A converter that replaces null values with a specified replacement. Can be used for the SelectedItem
    /// of 'nullable' comboboxes (together with <c>EnumerableAddDefaultItemConverter</c>).
    /// </summary>
    public class NullReplaceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null || value.Equals(parameter)) ? null : value;
        }
    }
}

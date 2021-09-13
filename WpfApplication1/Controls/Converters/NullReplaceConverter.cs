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
    /// A converter that replaces null values with a specified default. Can be used for the SelectedItem of 'nullable' 
    /// comboboxes (together with <see cref="EnumerableAddDefaultItemConverter">EnumerableAddDefaultItemConverter</see>).
    /// </summary>
    public class NullReplaceConverter : IValueConverter
    {
        /// <summary>
        /// Replaces null values with a specified default value.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">The default value to use when value is null</param>
        /// <param name="culture"></param>
        /// <returns>The value if it is not null, else the default value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }

        /// <summary>
        /// Converts values back. When a value matches the default for null values, null is returned instead.
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">The default value for null</param>
        /// <param name="culture"></param>
        /// <returns>The value if it is not the null default value, else null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null || value.Equals(parameter)) ? null : value;
        }
    }
}

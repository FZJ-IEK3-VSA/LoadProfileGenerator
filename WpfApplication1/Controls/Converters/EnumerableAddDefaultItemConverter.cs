using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LoadProfileGenerator.Controls.Converters
{
    /// <summary>
    /// A converter that adds a specified parameter as first element to the enumerable. Can be used to add
    /// a default/null value to comboboxes.
    /// </summary>
    public class EnumerableAddDefaultItemConverter : IValueConverter
    {
        /// <summary>
        /// Adds a specified default value to the received enumerable.
        /// </summary>
        /// <param name="value">An enumerable</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">The value to add to the enumerable</param>
        /// <param name="culture"></param>
        /// <returns>The extended enumerable</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = (IEnumerable)value;
            return new object[] { parameter }.Concat(collection.Cast<object>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

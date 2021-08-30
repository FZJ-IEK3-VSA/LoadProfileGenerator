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

using Database.Helpers;
using Database.Tables;
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
    /// A converter that receives an object ID and uses that to look up the object name.
    /// </summary>
    class IDToNameConverter : IMultiValueConverter
    {

        /// <summary>
        /// Looks up the passed object ID using a list of all objects.
        /// </summary>
        /// <param name="values">Contains two elements: the first one is the object ID as <c>int?</c>, the second
        /// one is a list of all objects of the same type (has to be an <see cref="IEnumerable{DBBase}">Enumerable of DBBase</see>),
        /// which should contain the object with the specified ID.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">The default value to use when the object with the specified ID was not found</param>
        /// <param name="culture"></param>
        /// <returns>The name of the object</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int? id = (int?)values[0];
            var list = (IEnumerable<DBBase>)values[1];
            return list.FirstOrDefault(x => x.IntID == id)?.Name ?? parameter;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

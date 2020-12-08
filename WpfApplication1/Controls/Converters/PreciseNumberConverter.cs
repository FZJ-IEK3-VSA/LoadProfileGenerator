using System;
using System.Globalization;
using System.Windows.Data;
using Common;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Controls.Converters {
    public class PreciseNumberConverter : IValueConverter {
        #region IValueConverter Members

        [JetBrains.Annotations.NotNull]
        public object Convert(object value, [CanBeNull] Type targetType, object parameter,
                              [JetBrains.Annotations.NotNull] CultureInfo culture) {
            string result;
            if (value is decimal d1) {
                result = d1.ToString("0.000000", CultureInfo.CurrentCulture);
            }
            else if (value is double d2)
            {
                result = d2.ToString("0.000000", CultureInfo.CurrentCulture);
            }
            else
            {
                result = value as string;
            }
            if (result == null) {
                result = string.Empty;
            }
            return result;
        }

        [JetBrains.Annotations.NotNull]
        public object ConvertBack(object value, [CanBeNull] Type targetType, object parameter,
                                  [JetBrains.Annotations.NotNull] CultureInfo culture) {
            if (value == null) {
                return 0;
            }
            var myValue = value.ToString();
            if (targetType == typeof(decimal)) {
                var success = decimal.TryParse(myValue, out decimal d);

                if (!success) {
                    Logger.Error("Could not convert " + myValue + " to decimal.");
                }
                return d;
            }
            if (targetType == typeof(double)) {
                var success = double.TryParse(myValue, out double d);
                if (!success) {
                    Logger.Error("Could not convert " + myValue + " to double.");
                }
                return d;
            }
            throw new LPGNotImplementedException();
        }

        #endregion
    }
}
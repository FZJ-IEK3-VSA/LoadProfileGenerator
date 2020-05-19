using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Presenters.Households {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class EnumBooleanConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, [CanBeNull] Type targetType, object parameter,
            [CanBeNull] CultureInfo culture) {
            if (!(parameter is string parameterString))
            {
                return DependencyProperty.UnsetValue;
            }
            if (value == null) {
                return 0;
            }
            if (!Enum.IsDefined(value.GetType(), value)) {
                return DependencyProperty.UnsetValue;
            }

            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        public object ConvertBack(object value, [CanBeNull] Type targetType, object parameter,
            [CanBeNull] CultureInfo culture) {
            if (!(parameter is string parameterString)) {
                return DependencyProperty.UnsetValue;
            }
            if (targetType == null) {
                return 0;
            }
            return Enum.Parse(targetType, parameterString);
        }

        #endregion
    }
}
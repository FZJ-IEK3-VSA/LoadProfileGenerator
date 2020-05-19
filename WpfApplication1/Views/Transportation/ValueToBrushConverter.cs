using System;
using System.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Common;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    [ValueConversion(typeof(DataGridCell), typeof(Brush))]
    public sealed class ValueToBrushConverter : IValueConverter
    {
        [NotNull] public static readonly ValueToBrushConverter Default = new ValueToBrushConverter();

        public object Convert(object value, [NotNull] Type targetType, object parameter, [NotNull] System.Globalization.CultureInfo culture)
        {
            string input = string.Empty;
            try
            {
                DataGridCell dgc = (DataGridCell)value;
                if (dgc == null) {
                    return Brushes.Green;
                }

                if (dgc.DataContext is DataRowView rowView && dgc.Column != null) {
                    input = (string) rowView.Row.ItemArray[dgc.Column.DisplayIndex];
                    Logger.Info(input);
                }
            }
            catch (InvalidCastException)
            {
                return DependencyProperty.UnsetValue;
            }
            switch (input)
            {
                case "1": return Brushes.Green;
                case "0": return Brushes.Red;
                default: return Brushes.Aqua;
            }
        }

        public object ConvertBack(object value, [NotNull] Type targetType, object parameter, [NotNull] System.Globalization.CultureInfo culture) => throw new NotSupportedException();
    }
}
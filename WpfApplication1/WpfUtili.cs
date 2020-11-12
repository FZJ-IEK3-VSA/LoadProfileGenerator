using System;
using Common;
using JetBrains.Annotations;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace LoadProfileGenerator
{
    public static class WpfUtili
    {
        public static bool CheckTextBox([NotNull] this TextBox txtBox, [NotNull] string errorMessage)
        {
            var allGood = !String.IsNullOrWhiteSpace(txtBox.Text);
            if (!allGood)
            {
                Logger.Error(errorMessage);
            }
            return allGood;
        }

        public static bool CheckCombobox([NotNull] this ComboBox comboBox, [NotNull] string errorMessage)
        {
            var allGood = comboBox.SelectedItem != null;
            if (!allGood)
            {
                Logger.Error(errorMessage);
            }
            return allGood;
        }
    }
}

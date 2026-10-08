using System;
using Common;
using ComboBox = System.Windows.Controls.ComboBox;
using TextBox = System.Windows.Controls.TextBox;

namespace LoadProfileGenerator
{
    public static class WpfUtili
    {
        public static bool CheckTextBox([JetBrains.Annotations.NotNull] this TextBox txtBox, [JetBrains.Annotations.NotNull] string errorMessage)
        {
            var allGood = !String.IsNullOrWhiteSpace(txtBox.Text);
            if (!allGood)
            {
                Logger.Error(errorMessage);
            }
            return allGood;
        }

        public static bool CheckCombobox([JetBrains.Annotations.NotNull] this ComboBox comboBox, [JetBrains.Annotations.NotNull] string errorMessage)
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

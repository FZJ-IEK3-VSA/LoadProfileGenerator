using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace LoadProfileGenerator
{
    /// <summary>
    ///     Interaktionslogik für InputBox.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class InputBox
    {
        public InputBox()
        {
            InitializeComponent();

            MyGrid.DataContext = this;
            IsOk = false;
        }

        [UsedImplicitly]
        public bool IsOk { get; set; }

        [UsedImplicitly]
        [NotNull]
        public string Result { get; set; }

        private void CancelClick([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e) => Close();

        private void InputBox_OnLoaded([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e) => TxtTag.Focus();

        private void OkClick([CanBeNull]object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                return;
            }
            IsOk = true;
            Close();
        }

        private void TxtTag_OnKeyUp([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrWhiteSpace(TxtTag.Text))
                {
                    return;
                }

                IsOk = true;
                Close();
            }
        }
    }
}
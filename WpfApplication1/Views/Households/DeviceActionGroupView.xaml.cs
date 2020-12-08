using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik für DeviceActionGroup.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class DeviceActionGroupView {
        public DeviceActionGroupView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private DeviceActionGroupPresenter Presenter => (DeviceActionGroupPresenter) DataContext;

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisDeviceActionGroup.Name, Presenter.Delete);

        private void LstPersonDesires_MouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }
    }
}
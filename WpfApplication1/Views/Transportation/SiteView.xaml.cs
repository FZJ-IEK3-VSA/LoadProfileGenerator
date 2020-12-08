using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class SiteView {
        public SiteView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private SitePresenter Presenter => (SitePresenter) DataContext;

        private void BtnAddLocationClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbLocations.SelectedItem == null) {
                Logger.Error("Please select a location first!");
                return;
            }

            if (!(CmbLocations.SelectedItem is Location tp)) {
                Logger.Error("Please select a location first!");
                return;
            }

            Presenter.ThisSite.AddLocation(tp);
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshUsedIn();

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstLocations.SelectedItem == null) {
                Logger.Error("Please select a location first!");
                return;
            }

            if (!(LstLocations.SelectedItem is SiteLocation dap)) {
                Logger.Error("Please select a location first!");
                return;
            }

            Presenter.ThisSite.DeleteLocation(dap);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisSite.Name, Presenter.Delete);

        private void LstLocations_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstLocations.SelectedItem as SiteLocation;
            if (ui?.Location != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Location);
            }
            else {
                Logger.Error("Please click on a location.");
            }
        }

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                Logger.Error("Please click on a person.");
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void BtnCopySiteClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.MakeCopy();
        }
    }
}
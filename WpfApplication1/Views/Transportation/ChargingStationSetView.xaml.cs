using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Common;
using Database.Tables.BasicHouseholds;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class ChargingStationSetView
    {
        public ChargingStationSetView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private ChargingStationSetPresenter Presenter => (ChargingStationSetPresenter) DataContext;

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisChargingStationSet.Name, Presenter.Delete);

        private void AddChargingStation([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            if ((CmbDeviceCategories.SelectedItem == null)|| !(CmbDeviceCategories.SelectedItem is TransportationDeviceCategory devcat))
            {
                Logger.Error("Please select a travel route first!");
                return;
            }

            if ((CmbSites.SelectedItem == null) || !(CmbSites.SelectedItem is Site site))
            {
                Logger.Error("Please select a site first!");
                return;
            }
            if ((CmbCarLoadTypes.SelectedItem == null) || !(CmbCarLoadTypes.SelectedItem is VLoadType carloadType))
            {
                Logger.Error("Please select a car load type first!");
                return;
            }
            if ((CmbGridLoadtype.SelectedItem == null) || !(CmbGridLoadtype.SelectedItem is VLoadType gridloadType))
            {
                Logger.Error("Please select a grid load type first!");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtMaxChargingPower.Text)) {
                Logger.Error("Please enter a max charging power first!");
                return;
            }
            bool success = double.TryParse(TxtMaxChargingPower.Text, out double maxcharging);
            if(!success)
            {
                Logger.Error("Please enter a max charging power first!");
                return;
            }
            Presenter.ThisChargingStationSet.AddChargingStation(devcat,
                carloadType,maxcharging,site,gridloadType);
        }

        private void BtnRemoveChargingStationClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            if (LstChargingStations.SelectedItem == null) {
                Logger.Error("Please select a charging station to delete first!");
                return;
            }

            ChargingStationSetEntry csse = (ChargingStationSetEntry)LstChargingStations.SelectedItem;
            Presenter.ThisChargingStationSet.DeleteChargingStation(csse);
        }

        private void BtnCopyChargingStationClick(object sender, RoutedEventArgs e)
        {
            var other = Presenter.ThisChargingStationSet.MakeExactCopy(Presenter.Sim);
            Presenter.ApplicationPresenter.OpenItem(other);
        }
    }
}
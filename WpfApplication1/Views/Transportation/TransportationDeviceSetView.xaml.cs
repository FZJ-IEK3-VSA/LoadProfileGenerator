using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TransportationDeviceSetView {
        public TransportationDeviceSetView()
        {
            InitializeComponent();
        }

        [NotNull]
        private TransportationDeviceSetPresenter Presenter => (TransportationDeviceSetPresenter) DataContext;

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            //TODO: implement
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisSet.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void BtnAddTransportationDevice_Click([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (CmbTransportationDevices.SelectedItem == null) {
                Logger.Error("No transportation device was selected");
                return;
            }

            TransportationDevice td = (TransportationDevice) CmbTransportationDevices.SelectedItem;
            Presenter.ThisSet.AddDevice(td);
        }

        private void BtnRemoveTransportationDevice_Click([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            if (LstTransportationDevices.SelectedItem == null) {
                Logger.Error("No transportation device was selected.");
                return;
            }

            TransportationDeviceSetEntry entry = (TransportationDeviceSetEntry) LstTransportationDevices.SelectedItem;
            Presenter.ThisSet.DeleteEntry(entry);
        }
    }
}
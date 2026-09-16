using System;
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
    public partial class TransportationDeviceView {
        public TransportationDeviceView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TransportationDevicePresenter Presenter => (TransportationDevicePresenter) DataContext;

        private void BtnAddLoad_Click([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            string loadStr = TxtLoadValue.Text;
            bool success = double.TryParse(loadStr, out double load);
            if (!success) {
                Logger.Error("The maximum power was not a number.");
                return;
            }

            VLoadType lt = (VLoadType) CmbLoadTypes.SelectedItem;
            if (lt == null) {
                Logger.Error("No load type was selected. Please fix.");
                return;
            }

            Presenter.ThisTransportationDevice.AddLoad(lt, load);
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            //TODO: implement
#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
            throw new NotImplementedException();
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
        }

        private void BtnRemoveLoad_Click([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            if (LstLoads.SelectedItem == null) {
                Logger.Error("No item was selected");
                return;
            }

            TransportationDeviceLoad l = (TransportationDeviceLoad) LstLoads.SelectedItem;
            Presenter.ThisTransportationDevice.DeleteLoad(l);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisTransportationDevice.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }
    }
}
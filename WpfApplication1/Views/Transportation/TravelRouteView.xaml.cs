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
    public partial class TravelRouteView {
        public TravelRouteView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TravelRoutePresenter Presenter => (TravelRoutePresenter) DataContext;

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTransportationDeviceCategory.SelectedItem == null) {
                Logger.Error("Please select a transportation device category first!");
                return;
            }

            string distanceStr = TxtDistance.Text;
            bool success = double.TryParse(distanceStr, out var distance);
            if (!success) {
                Logger.Error("Could not parse the distance!");
                return;
            }

            string stepStr = TxtStepNumber.Text;
            success = int.TryParse(stepStr, out var step);
            if (!success) {
                Logger.Error("Could not parse the step number!");
                return;
            }

            if (!(CmbTransportationDeviceCategory.SelectedItem is TransportationDeviceCategory tp)) {
                Logger.Error("Could not add the transportation device category.");
                return;
            }

            string name = TxtStepName.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                Logger.Error("No name was entered");
                return;
            }

            string stepKey = TxtStepKey.Text;

            Presenter.ThisRoute.AddStep(name, tp, distance, step, stepKey);
            LstSteps.ResizeColummns();
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            TravelRoutePresenter.RefreshUsedIn();

        private void BtnRemoveDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstSteps.SelectedItem == null) {
                Logger.Error("Please select a step.");
                return;
            }

            if (!(LstSteps.SelectedItem is TravelRouteStep dap)) {
                Logger.Error("Could not add the step");
                return;
            }

            Presenter.ThisRoute.DeleteStep(dap);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisRoute.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                Logger.Error("Please select a desire.");
                return;
            }

            UsedIn ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void BtnSort([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.ThisRoute.Steps.Sort();
        }

        private void BtnCopyRouteClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.CopyRoute();
        }
    }
}
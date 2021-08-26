using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.Transportation;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TravelRouteSetView {
        public TravelRouteSetView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private TravelRouteSetPresenter Presenter => (TravelRouteSetPresenter) DataContext;

        private void BtnAddDeviceClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbTravelRoutes.SelectedItem == null) {
                Logger.Error("Please select a travel route first!");
                return;
            }

            if (!(CmbTravelRoutes.SelectedItem is TravelRoute tp)) {
                Logger.Error("Please select a travel route first!");
                return;
            }

            if (Presenter.Weight < 0.0)
            {
                Logger.Error("Please enter a valid weight (weights must be >= 0)");
                return;
            }

            // only specify some of the parameters, according to the selected input type
            var inputType = Presenter.InputOptions[Presenter.SelectedInputOption];
            switch (inputType)
            {
                case TravelRouteEntryInputOption.AgeAndGender:
                    Presenter.ThisRouteSet.AddRoute(tp, minimumAge: Presenter.MinimumAge, maximumAge: Presenter.MaximumAge, gender: Presenter.SelectedGender,
                        affordanceTag: Presenter.SelectedAffordanceTag, weight: Presenter.Weight);
                    break;
                case TravelRouteEntryInputOption.Person:
                    Presenter.ThisRouteSet.AddRoute(tp, affordanceTag: Presenter.SelectedAffordanceTag, personID: Presenter.SelectedPerson?.IntID,
                        weight: Presenter.Weight);
                    break;
                default:
                    throw new LPGNotImplementedException("Not implemented for all items of enum " + nameof(TravelRouteEntryInputOption));
            }

            Presenter.RefreshRoutes();
        }

        private void BtnFindMissingSitesClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.FindMissingRoutesAndCreateThem();
        }

        private void BtnRefreshRoutes([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.RefreshRoutes();
        }

        private void BtnRefreshSitesClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.RefreshDataTable();
        }

        private void BtnRefreshUsedIn_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            TravelRouteSetPresenter.RefreshUsedIn();

        private void BtnRemoveRoutesClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (!(LstSteps.SelectedItem is TravelRouteSetEntry entry)) {
                Logger.Error("Please select a TravelRoute entry first!");
                return;
            }

            Presenter.ThisRouteSet.DeleteEntry(entry);
            Presenter.RefreshRoutes();
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.AskDeleteQuestion(
                Presenter.ThisRouteSet.Name, Presenter.Delete);

        private void LstPersonDesiresMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstPersonDesires.SelectedItem == null) {
                return;
            }

            var ui = (UsedIn) LstPersonDesires.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(ui.Item);
        }

        private void LstSteps_OnMouseDoubleClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] MouseButtonEventArgs e)
        {
            if (LstSteps.SelectedItem == null) {
                return;
            }

            var entry = (TravelRouteSetEntry) LstSteps.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(entry.TravelRoute);
        }

        private void BtnFindMissingSitesForAllHouseholdsClick([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            Presenter.FindMissingRoutesForAllHouseholds();
        }

        private void BtnMakeCopy(object sender, RoutedEventArgs e)
        {
            Presenter.MakeCopy();
        }

        private void BtnRemoveWorkplaceRoutes(object sender, RoutedEventArgs e)
        {
            Presenter.RemoveWorkplaceRoutes();
            Presenter.RefreshRoutes();
        }

        private void BtnAddDistanceWorkplaceRoute(object sender, RoutedEventArgs e)
        {
            Presenter.AddDistanceMatchingWorkplaceRoutes();
            Presenter.RefreshRoutes();
        }
    }
}
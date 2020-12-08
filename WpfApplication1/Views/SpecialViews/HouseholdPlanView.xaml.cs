using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Database.Tables.BasicElements;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.SpecialViews {
    /// <summary>
    ///     Interaktionslogik für HouseholdPlanView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HouseholdPlanView {
        [CanBeNull] private ProgressbarWindow _pbw;

        public HouseholdPlanView() {
            var t = new Thread(() => {
                _pbw = new ProgressbarWindow("Loading household plan...",
                    "Loading the household plan. This takes a moment unfortunately...", 1);
                _pbw.ShowDialog();
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private HouseholdPlanPresenter Presenter => (HouseholdPlanPresenter)DataContext;

        private void ALAddClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (CmbAlPersons.SelectedItem == null) {
                return;
            }
            if (CmbAlTraits.SelectedItem == null) {
                return;
            }
            Presenter.AddAlTrait();
        }

        private void ALRefresh_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshAlAssignments();

        private void ALRemoveBtn_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstAl.SelectedItem == null) {
                return;
            }
            var ala = (ModularHouseholdTrait) LstAl.SelectedItem;
            Presenter.RemoveAlTrait(ala);
        }

        private void BtnRefreshClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshPlan();

        private void BtnRefreshStatisticsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshStatistics();

        private void BtnResortClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Resort();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisHouseholdPlan.HeaderString, Presenter.Delete);

        private void HouseholdPlanView_OnLoaded([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (_pbw != null) {
                Action close = () => {
                    if (_pbw.IsVisible) {
                        _pbw.Close();
                    }
                };
                _pbw.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, close);
            }
            Presenter.SuspendStatistics = false;
            Presenter.RefreshStatistics();
        }

        [SuppressMessage("ReSharper", "AccessToStaticMemberViaDerivedType")]
        private void RefreshCHousehold_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            Presenter.RefreshCHousehold();
            LstEntries.InvalidateProperty(ListView.ItemsSourceProperty);
        }

        private void RefreshModularHouseholdNumbers([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.RefreshFromTraits();

        private void TraitAddClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (e == null) {
                return;
            }
            var b = (Button) e.Source;
            var entry = (HouseholdPlanEntry) b.CommandParameter;
            Presenter.AddTraitToModularHousehold(entry);
        }

        private void TraitOpenClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (e == null)
            {
                return;
            }
            var b = (Button) e.Source;
            var entry = (HouseholdPlanEntry) b.CommandParameter;
            Presenter.ApplicationPresenter.OpenItem(entry.SelectedTrait);
        }
    }
}
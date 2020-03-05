using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Common;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.BasicElements {
    /// <summary>
    ///     Interaktionslogik für VacationView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class VacationView {
        public VacationView() {
            InitializeComponent();
        }
        [NotNull]
        private VacationPresenter Presenter => (VacationPresenter)DataContext;

        private void BtnAddDate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (string.IsNullOrEmpty(TxtStart.Text)) {
                return;
            }
            if (string.IsNullOrEmpty(TxtEnd.Text)) {
                return;
            }
            var success = DateTime.TryParse(TxtStart.Text, out var start);
            if (!success) {
                Logger.Error("Start date was not recognized as date.");
                return;
            }
            success = DateTime.TryParse(TxtEnd.Text, out DateTime end);
            if (!success) {
                Logger.Error("End date was not recognized as date.");
                return;
            }
            Presenter.ThisVacation.AddVacationTime(start, end, Presenter.SelectedVacationType);
        }

        private void BtnRemoveDate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstVacations.SelectedItem == null) {
                return;
            }
            var vt = (VacationTime) LstVacations.SelectedItem;
            Presenter.ThisVacation.DeleteVacationTime(vt);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisVacation.HeaderString, Presenter.Delete);
    }
}
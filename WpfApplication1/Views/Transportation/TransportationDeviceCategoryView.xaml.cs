using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Database.Tables.BasicElements;
using JetBrains.Annotations;

namespace LoadProfileGenerator.Views.Transportation {
    /// <summary>
    ///     Interaktionslogik für DeviceActionView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class TransportationDeviceCategoryView {
        public TransportationDeviceCategoryView()
        {
            InitializeComponent();
        }

        [NotNull]
        private TransportationDeviceCategoryPresenter Presenter => (TransportationDeviceCategoryPresenter) DataContext;

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
                Presenter.ThisCategory.Name, Presenter.Delete);

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
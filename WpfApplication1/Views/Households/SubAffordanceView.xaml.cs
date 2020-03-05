//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

#region

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Common.Enums;
using Database.Tables.BasicElements;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Households;

#endregion

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r SubAffordanceView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class SubAffordanceView {
        public SubAffordanceView()
        {
            InitializeComponent();
        }

        [NotNull]
        private SubAffordancePresenter Presenter => (SubAffordancePresenter) DataContext;

        private void BtnAddDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbDesires.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtSatisfactionValue.Text)) {
                return;
            }

            var d = Utili.ConvertToDecimalWithMessage(TxtSatisfactionValue.Text);
            d /= 100;
            Presenter.AddDesire((Desire) CmbDesires.SelectedItem, d);
        }

        private void BtnAddVariableOpClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbOpVariable.SelectedItem == null) {
                return;
            }

            if (string.IsNullOrEmpty(TxtOpVariableValue.Text)) {
                return;
            }

            if (CmbOpAction.SelectedItem == null) {
                return;
            }

            if (CmbOpExecutionTime.SelectedItem == null) {
                return;
            }

            Location loc = null;
            if (CmbOpLocation.SelectedItem != null) {
                loc = (Location) CmbOpLocation.SelectedItem;
            }

            var mode =
                VariableLocationModeHelper.ConvertToVariableAction((string) CmbOpLocationMode.SelectedItem);
            var val = Utili.ConvertToDoubleWithMessage(TxtOpVariableValue.Text);
            var va = (Variable) CmbOpVariable.SelectedItem;
            var variableAction =
                VariableActionHelper.ConvertToVariableAction((string) CmbOpAction.SelectedItem);
            var executionTime =
                VariableExecutionTimeHelper.ConvertToEnum((string) CmbOpExecutionTime.SelectedItem);
            Presenter.ThisSubAffordance.AddVariableOperation(val, mode, loc, variableAction, va, executionTime);
        }

        private void BtnRefreshHouseholds_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIn();
            LstUsedIn.ResizeColummns();
        }

        private void BtnRemoveDesireClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstAffordanceDesires.SelectedItem == null) {
                return;
            }

            Presenter.RemoveDesire((SubAffordanceDesire) LstAffordanceDesires.SelectedItem);
        }

        private void BtnRemoveVariableOpClick([NotNull] object sender, [NotNull] RoutedEventArgs routedEventArgs)
        {
            if (LstOpVariables.SelectedItem == null) {
                return;
            }

            var mytrigger = (SubAffordanceVariableOp) LstOpVariables.SelectedItem;
            Presenter.ThisSubAffordance.DeleteVariableOpFromDB(mytrigger);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisSubAffordance.HeaderString, Presenter.Delete);

        private void LstAffordanceDesires_OnMouseDoubleClick([CanBeNull] object sender,
            [CanBeNull] MouseButtonEventArgs e)
        {
            var ad = LstAffordanceDesires.SelectedItem as SubAffordanceDesire;
            if (ad?.Desire != null) {
                Presenter.ApplicationPresenter.OpenItem(ad.Desire);
            }
        }

        private void LstUsedByMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            var ui = LstUsedIn.SelectedItem as UsedIn;
            if (ui?.Item != null) {
                Presenter.ApplicationPresenter.OpenItem(ui.Item);
            }
        }
    }
}
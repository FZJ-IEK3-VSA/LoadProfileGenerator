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

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using Database.Tables.Houses;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.Houses;

namespace LoadProfileGenerator.Views.Houses {
    /// <summary>
    ///     Interaktionslogik f�r EnergyStorageView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class EnergyStorageView {
        public EnergyStorageView()
        {
            InitializeComponent();
        }

        [NotNull]
        private EnergyStoragePresenter Presenter => (EnergyStoragePresenter) DataContext;

        private void BtnAddSignal_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            var isOk = CmbSignalVariable.SelectedItem != null;
            if (string.IsNullOrEmpty(TxtTriggerLevelOff.Text)) {
                isOk = false;
            }

            if (string.IsNullOrEmpty(TxtTriggerLevelOn.Text)) {
                isOk = false;
            }

            if (string.IsNullOrEmpty(TxtSignalValue.Text)) {
                isOk = false;
            }

            if (!isOk) {
                Logger.Error("All fields need to be filled in to add a signal.");
                return;
            }

            var lt = (Variable) CmbSignalVariable.SelectedItem;
            var off = Utili.ConvertToDoubleWithMessage(TxtTriggerLevelOff.Text);
            var on = Utili.ConvertToDoubleWithMessage(TxtTriggerLevelOn.Text);
            var value = Utili.ConvertToDoubleWithMessage(TxtSignalValue.Text);
            Presenter.ThisStorage.AddSignal(lt, on, off, value);
            LstSignals.ResizeColummns();
        }

        private void BtnRefreshUsedInClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            Presenter.RefreshUsedIns();
        }

        private void ButtonBase_OnClicktnRemoveSignal_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstSignals.SelectedItem == null) {
                return;
            }

            var ess = (EnergyStorageSignal) LstSignals.SelectedItem;
            Presenter.ThisStorage.RemoveSignal(ess);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisStorage.HeaderString, Presenter.Delete);

        private void LstUsedInMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstUsedIns.SelectedItem == null) {
                return;
            }

            var pui = (UsedIn) LstUsedIns.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(pui.Item);
        }
    }
}
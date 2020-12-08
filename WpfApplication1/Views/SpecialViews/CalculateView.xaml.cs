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

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using CalculationController.Queue;
using Common;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;
using Microsoft.Win32;

#endregion

namespace LoadProfileGenerator.Views.SpecialViews {
    /// <summary>
    ///     Interaktionslogik f�r CalculateView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class CalculateView {
        public CalculateView()
        {
            InitializeComponent();
        }

        [JetBrains.Annotations.NotNull]
        private CalculationPresenter Presenter => (CalculationPresenter) DataContext;

        private void BtnCalculateClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbHouseholds.SelectedItem == null) {
                Logger.Error("Please select an item.");
                return;
            }

            var resultpath = TxtDstPath.Text;

            if (!Debugger.IsAttached) {
                // for debugging don't catch the exceptions
                try {
                    Presenter.RunSimulation(resultpath);
                }
                catch (Exception f) {
                    Logger.Exception(f);
                    if (!Config.IsInUnitTesting) {
                        MessageWindowHandler.Mw.ShowDebugMessage(f);
                    }
                    else {
                        throw;
                    }
                }
            }
            else {
                Presenter.RunSimulation(resultpath);
            }
        }

        private void BtnCancelClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            CalcStarter.CancelRun();
            MessageWindowHandler.Mw.ShowInfoMessage(
                "Stopping calculation... please wait a moment for the current step to finish.", "Please wait.");
            Logger.Info("Canceled the Calculation...");
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void TargetRefresh_OnClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) =>
            Presenter.RefreshTargets();

        private void BtnExportJson([JetBrains.Annotations.NotNull] object sender, [JetBrains.Annotations.NotNull] RoutedEventArgs e)
        {
            var ofd = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "Json Files (*.json)|*.json|All files (*.*)|*.*"
            };
            var result = ofd.ShowDialog();
            if (result == true) {
                var filename = ofd.FileName;
                string s = Presenter.WriteCalculationJsonSpecForCommandLine(filename);
                MessageWindowHandler.Mw.ShowInfoMessage(s, "Export");
            }
        }
    }
}
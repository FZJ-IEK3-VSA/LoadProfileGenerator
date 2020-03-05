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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using CalculationController.Queue;
using Database.Tables.Validation;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.SpecialViews;

namespace LoadProfileGenerator.Views.SpecialViews {
    /// <summary>
    ///     Interaktionslogik f�r UnusedDevicesView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class CalculationOutcomesView {
        public CalculationOutcomesView() {
            InitializeComponent();
        }

        [NotNull]
        private CalculationOutcomesPresenter Presenter => (CalculationOutcomesPresenter) DataContext;

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void DeleteEmptyClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.DeleteEmptyOutcomes();

        private void DeleteSelectedResultClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstOutcomes.SelectedItem == null) {
                return;
            }
            var outcomes = new List<CalculationOutcome>();
            foreach (var item in LstOutcomes.SelectedItems) {
                var co = (CalculationOutcome) item;
                outcomes.Add(co);
            }

            foreach (var co in outcomes) {
                Presenter.DeleteOutcome(co);
            }
        }

        private void DuplicateCheckClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.FindDuplicates();

        private void ExportClickClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(Presenter.CSVPath)) {
                Presenter.ExportToCSV();
            }
        }

        private void MakeVersionComparisonChart([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            CalculationOutcomesPresenter.MakeVersionComparisonChart(Presenter.Sim);
        }

        private void StartCalculationsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.StartCalculations(false);

        private void StartCountCalculationsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.StartCalculations(true);

        private void StopCalculationsClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            CalcStarter.CancelRun();
            Presenter.InCalculation = false;
        }

        private void TxtFilter_OnKeyUp([NotNull] object sender, [NotNull] KeyEventArgs e) => Presenter.FilterString = TxtFilter.Text;
    }
}
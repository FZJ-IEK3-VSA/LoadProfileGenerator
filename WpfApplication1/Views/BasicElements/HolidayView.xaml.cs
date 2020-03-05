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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Input;
using Common;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.BasicElements {
    /// <summary>
    ///     Interaktionslogik f�r HolidayView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class HolidayView {
        public HolidayView() {
            InitializeComponent();
        }

        [NotNull]
        private HolidayPresenter Presenter => (HolidayPresenter)DataContext;

        private void BtnAddDate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            var s = TxtDate.Text;
            if (string.IsNullOrEmpty(s)) {
                Logger.Error("You need to enter a date!");
                return;
            }
            if (!DateTime.TryParse(s, out var dt)) {
                Logger.Error("The date was invalid");
                return;
            }
            Presenter.ThisHoliday.AddNewDate(dt);
        }

        private void BtnRemoveDate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) {
            if (LstHolidayDates.SelectedItem == null) {
                Logger.Error("You need to select a date!");
                return;
            }
            var holidayDate = (HolidayDate) LstHolidayDates.SelectedItem;
            Presenter.ThisHoliday.DeleteHoliday(holidayDate);
        }

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.AskDeleteQuestion(
            Presenter.ThisHoliday.HeaderString, Presenter.Delete);

        private void TxtDate_OnKeyUp([CanBeNull] object sender, [CanBeNull] KeyEventArgs e)
        {
            if (e == null) {
                return;
            }
            if (e.Key != Key.Enter) {
                return;
            }
            var s = TxtDate.Text;
            if (string.IsNullOrEmpty(s)) {
                Logger.Error("You need to enter a date!");
                return;
            }
            if (!DateTime.TryParse(s, out var dt)) {
                Logger.Error("The date was invalid");
                return;
            }
            Presenter.ThisHoliday.AddNewDate(dt);
        }
    }
}
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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Presenters.BasicElements;

namespace LoadProfileGenerator.Views.Households {
    /// <summary>
    ///     Interaktionslogik f�r GeographicLocationView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public partial class GeographicLocationView {
        public GeographicLocationView()
        {
            InitializeComponent();
        }

        [NotNull]
        private GeographicLocationPresenter Presenter => (GeographicLocationPresenter) DataContext;

        private void BtnAddHoliday_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (CmbHoliday.SelectedItem == null) {
                return;
            }

            var hd = (Holiday) CmbHoliday.SelectedItem;
            Presenter.ThisGeographicLocation.AddHoliday(hd);
        }

        private void BtnImport_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (Presenter.SelectedGeoLoc == null) {
                return;
            }

            var holidays = Presenter.SelectedGeoLoc.Holidays.ToList();

            foreach (var holiday in holidays) {
                Presenter.ThisGeographicLocation.AddHoliday(holiday.Holiday);
            }
        }

        private void BtnRemoveDate_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
        {
            if (LstHolidays.SelectedItem == null) {
                return;
            }

            var geoholi = (GeographicLocHoliday) LstHolidays.SelectedItem;
            Presenter.ThisGeographicLocation.DeleteGeoHolidayFromDB(geoholi);
        }

        private void BtnReplaceWithNoBridgeDaysDates_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.ReplaceAllHolidaysNoBridgeDays();

        private void CloseClick([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e) => Presenter.Close(true);

        private void Delete_Click([CanBeNull] object sender, [CanBeNull] RoutedEventArgs e)
            => Presenter.AskDeleteQuestion(Presenter.ThisGeographicLocation.HeaderString, Presenter.Delete);

        private void LstHolidays_OnMouseDoubleClick([CanBeNull] object sender, [CanBeNull] MouseButtonEventArgs e)
        {
            if (LstHolidays.SelectedItem == null) {
                return;
            }

            var item = (GeographicLocHoliday) LstHolidays.SelectedItem;
            Presenter.ApplicationPresenter.OpenItem(item.Holiday);
        }
    }
}
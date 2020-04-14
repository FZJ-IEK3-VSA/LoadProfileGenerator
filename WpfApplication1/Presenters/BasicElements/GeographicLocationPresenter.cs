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
using System.Collections.ObjectModel;
using System.Linq;
using Automation.ResultFiles;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using LoadProfileGenerator.Views.Households;

namespace LoadProfileGenerator.Presenters.BasicElements {
    public class GeographicLocationPresenter : PresenterBaseDBBase<GeographicLocationView> {
        [NotNull] private readonly GeographicLocation _geoloc;

        [CanBeNull] private GeographicLocation _selectedGeoLoc;

        public GeographicLocationPresenter([NotNull] ApplicationPresenter applicationPresenter, [NotNull] GeographicLocationView view,
            [NotNull] GeographicLocation geoloc) : base(view, "ThisGeographicLocation.HeaderString", geoloc,
            applicationPresenter) => _geoloc = geoloc;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<GeographicLocation> GeoLocs => Sim.GeographicLocations.It;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<Holiday> Holidays => Sim.Holidays.MyItems;

        [CanBeNull]
        [UsedImplicitly]
        public GeographicLocation SelectedGeoLoc {
            get => _selectedGeoLoc;
            set {
                if (Equals(value, _selectedGeoLoc)) {
                    return;
                }
                _selectedGeoLoc = value;
                OnPropertyChanged(nameof(SelectedGeoLoc));
            }
        }

        [NotNull]
        public GeographicLocation ThisGeographicLocation => _geoloc;

        [ItemNotNull]
        [NotNull]
        [UsedImplicitly]
        public ObservableCollection<TimeLimit> TimeLimits => Sim.TimeLimits.MyItems;

        public override void Close(bool saveToDB, bool removeLast = false) {
            if (saveToDB) {
                _geoloc.SaveToDB();
            }
            ApplicationPresenter.CloseTab(this, removeLast);
        }

        public void Delete() {
            Sim.GeographicLocations.DeleteItem(_geoloc);
            Close(false);
        }

        public override bool Equals([CanBeNull] object obj) {
            return obj is GeographicLocationPresenter presenter && presenter.ThisGeographicLocation.Equals(_geoloc);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                // Suitable nullity checks etc, of course :)
                hash = hash * 23 + TabHeaderPath.GetHashCode();
                return hash;
            }
        }

        public void ReplaceAllHolidaysNoBridgeDays() {
            if (_selectedGeoLoc == null) {
                throw new LPGException("_selectedgeoloc was null");
            }
            var holidays = ThisGeographicLocation.Holidays.Select(x => x.Holiday).ToList();
            while (_geoloc.Holidays.Count > 0) {
                _geoloc.DeleteGeoHolidayFromDB(_geoloc.Holidays[0]);
            }
            foreach (var holiday in holidays) {
                if (holiday.Name.EndsWith(" (no bridge days)", StringComparison.Ordinal)) {
                    _geoloc.AddHoliday(holiday);
                    continue;
                }
                var newName = holiday.Name + " (no bridge days)";
                var hhd = Sim.Holidays.FindFirstByName(newName);
                if (hhd != null) {
                    _selectedGeoLoc.AddHoliday(hhd);
                    continue;
                }
                hhd = Sim.Holidays.CreateNewItem(Sim.ConnectionString);
                hhd.Name = newName;
                hhd.SaveToDB();
                foreach (var date in holiday.HolidayDates) {
                    hhd.AddNewDate(date.DateAndTime);
                }
                hhd.SaveToDB();
                _geoloc.AddHoliday(hhd);
            }
        }
    }
}
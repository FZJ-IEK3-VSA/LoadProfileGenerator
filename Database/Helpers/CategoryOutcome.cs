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

using System.Collections.ObjectModel;
using Automation;
using Database.Tables.BasicElements;
using Database.Tables.Validation;
using JetBrains.Annotations;

#endregion

namespace Database.Helpers {
    public class CategoryOutcome : CategoryDBBase<CalculationOutcome> {
        public CategoryOutcome() : base("Calculation Outcomes") {
        }

        [NotNull]
        [ItemNotNull]
        [UsedImplicitly]
        public new ObservableCollection<CategoryOutcome> FilteredItems { get; } =
            new ObservableCollection<CategoryOutcome>();

        public int CountItems([NotNull] string mycalcObject, [NotNull] string geoloc, [NotNull] string temperaturProfile, [NotNull] string intensity,
            [NotNull] string lpgVersion) {
            var count = 0;
            foreach (var co in MyItems) {
                if (co.HouseholdName == mycalcObject && co.GeographicLocationName == geoloc &&
                    co.TemperatureProfile == temperaturProfile && co.EnergyIntensity == intensity &&
                    co.LPGVersion == lpgVersion) {
                    count++;
                }
            }
            return count;
        }

        public bool ItemExists([NotNull] ICalcObject mycalcObject, [NotNull] GeographicLocation geoloc,
            [NotNull] TemperatureProfile temperatureProfile,
            EnergyIntensityType intensity, [NotNull] string lpgVersion) {
            foreach (var co in MyItems) {
                if (co.HouseholdName == mycalcObject.Name && co.GeographicLocationName == geoloc.Name &&
                    co.TemperatureProfile == temperatureProfile.Name && co.EnergyIntensity == intensity.ToString() &&
                    co.LPGVersion == lpgVersion) {
                    return true;
                }
            }
            return false;
        }
    }
}
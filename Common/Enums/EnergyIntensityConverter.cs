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
using Automation;
using Automation.ResultFiles;
using JetBrains.Annotations;

namespace Common.Enums {
    public class EnergyIntensityConverter {
        public EnergyIntensityConverter()
        {
            var eifd = new EnergyIntensityForDisplay(EnergyIntensityType.EnergyIntensive,
                "Energy Intensive");
            All.Add(eifd);
            ForHouseholds.Add(eifd);
            eifd = new EnergyIntensityForDisplay(EnergyIntensityType.EnergySaving,
                "Energy Saving");
            All.Add(eifd);
            ForHouseholds.Add(eifd);
            eifd = new EnergyIntensityForDisplay(EnergyIntensityType.Random,
                "Randomly chosen devices");
            All.Add(eifd);
            ForHouseholds.Add(eifd);
            eifd = new EnergyIntensityForDisplay(EnergyIntensityType.AsOriginal,
                "As chosen for the household");
            All.Add(eifd);
            eifd = new EnergyIntensityForDisplay(EnergyIntensityType.EnergyIntensivePreferMeasured,
                "Energy Intensive, but prefer measured devices if available");
            All.Add(eifd);
            ForHouseholds.Add(eifd);
            eifd = new EnergyIntensityForDisplay(EnergyIntensityType.EnergySavingPreferMeasured,
                "Energy Saving, but prefer measured devices if available");
            All.Add(eifd);
            ForHouseholds.Add(eifd);
        }

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<EnergyIntensityForDisplay> All { get; } =
            new ObservableCollection<EnergyIntensityForDisplay>();

        [NotNull]
        [ItemNotNull]
        public ObservableCollection<EnergyIntensityForDisplay> ForHouseholds { get; } =
            new ObservableCollection<EnergyIntensityForDisplay>();

        [NotNull]
        public EnergyIntensityForDisplay GetAllDisplayElement(EnergyIntensityType eit)
        {
            foreach (var energyIntensityForDisplay in All) {
                if (energyIntensityForDisplay.EnergyIntensityType == eit) {
                    return energyIntensityForDisplay;
                }
            }
            throw new LPGException("Energy intensity was not found");
        }

        public static EnergyIntensityType GetEnergyIntensityTypeFromString([NotNull] string energyIntensityStr)
        {
            foreach (EnergyIntensityType deviceType in Enum.GetValues(typeof(EnergyIntensityType))) {
                if (deviceType.ToString() == energyIntensityStr) {
                    return deviceType;
                }
            }
            return EnergyIntensityType.Random;
        }

        [NotNull]
        public EnergyIntensityForDisplay GetHHDisplayElement(EnergyIntensityType eit)
        {
            foreach (var energyIntensityForDisplay in ForHouseholds) {
                if (energyIntensityForDisplay.EnergyIntensityType == eit) {
                    return energyIntensityForDisplay;
                }
            }
            throw new LPGException("Energy intensity was not found!");
        }

        public class EnergyIntensityForDisplay {
            public EnergyIntensityForDisplay(EnergyIntensityType energyIntensityType, [NotNull] string name)
            {
                EnergyIntensityType = energyIntensityType;
                Name = name;
            }

            [UsedImplicitly]
            public EnergyIntensityType EnergyIntensityType { get; set; }

            [UsedImplicitly]
            [NotNull]
            public string Name { get; set; }

            [NotNull]
            public override string ToString() => Name;
        }
    }
}
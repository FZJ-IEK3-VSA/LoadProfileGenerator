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

using System.Collections.ObjectModel;
using System.IO;
using Automation;
using Automation.ResultFiles;
using Database;
using Database.Tables.Houses;
using Database.Tables.ModularHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Queue {
    public static class CalcEntryFactory {
        // this class creates the calculation entries
        public static void MakeCalcEntries([NotNull] CalcStartParameterSet csps,
            [NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries, [NotNull] string resultPath) {
            var co = csps.CalcTarget;
            if (co.GetType() == typeof(Settlement)) {
                MakeSettlementEntries((Settlement) co, resultPath, csps.EnergyIntensity, calculationEntries);
            }
            else if (co.GetType() == typeof(House) ||
                     co.GetType() == typeof(ModularHousehold)) {
                MakeOtherEntries(co, resultPath, csps.EnergyIntensity, calculationEntries);
            }
            else {
                throw new LPGException("Unknown what to calc type!");
            }
        }

        private static void MakeOtherEntries([NotNull] ICalcObject hh, [NotNull] string path, EnergyIntensityType energyIntensity,
            [NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) {
            var ce = new CalculationEntry(hh, path, 0)
            {
                EnergyIntensity = energyIntensity
            };
            calculationEntries.Add(ce);
        }

        private static void MakeSettlementEntries([NotNull] Settlement sett, [NotNull] string dstpath, EnergyIntensityType energyIntensity,
            [NotNull][ItemNotNull] ObservableCollection<CalculationEntry> calculationEntries) {
            if (!Directory.Exists(dstpath)) {
                Directory.CreateDirectory(dstpath);
            }

            using (var sw = new StreamWriter(Path.Combine(dstpath, "Information.txt"))) {
                sw.WriteLine(sett.Name);
                sw.WriteLine(energyIntensity.ToString());
                sw.WriteLine(sett.AllPersons.Count + " People");
                sw.WriteLine(sett.HouseholdCount + " Households:");

                foreach (var settlementHH in sett.Households) {
                    sw.WriteLine(settlementHH.CalcObject?.Name);
                }
            }
            foreach (var shh in sett.Households) {
                string name = shh.CalcObject?.Name;
                if (name == null) {
                    name = "no name";
                }
                var cleanName = AutomationUtili.CleanFileName(name);
                for (var i = 0; i < shh.Count; i++) {
                    var path = Path.Combine(dstpath, cleanName + " - #" + (i + 1));
                    if(shh.CalcObject == null) {
                        throw new LPGException("Calcobject was null");
                    }
                    var ce = new CalculationEntry(shh.CalcObject, path, i)
                    {
                        EnergyIntensity = energyIntensity
                    };
                    calculationEntries.Add(ce);
                }
            }
        }
    }
}
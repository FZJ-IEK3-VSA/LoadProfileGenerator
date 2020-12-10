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
using Common;
using Database;
using Database.Tables.Houses;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    public static class HouseIntegrityChecker {
        private static void CheckEnergyStorages([NotNull][ItemNotNull] ObservableCollection<EnergyStorage> myItems) {
            foreach (var storage1 in myItems) {
                foreach (var storage2 in myItems) {
                    if (storage1 != storage2 && storage1.Name == storage2.Name) {
                        throw new DataIntegrityException("There are two energy storages with the name " +
                                                         storage1.Name +
                                                         ". This is not permitted, since it would lead to really confusing result files.");
                    }
                }
            }
        }


        public static void Run([NotNull] House house, [NotNull] Simulator sim) {
            if (house.HouseType == null) {
                throw new DataIntegrityException("The house " + house.Name +
                                                 " has no house type selected. This makes calculation impossible.");
            }
            foreach (var energyStorage in house.HouseType.HouseEnergyStorages) {
                if (energyStorage.EnergyStorage?.LoadType == null) {
                    throw new DataIntegrityException("The energy storage " + energyStorage.EnergyStorage?.Name +
                                                     " has no load type selected. This makes calculation impossible.");
                }
            }
            CheckEnergyStorages(sim.EnergyStorages.Items);

        }
    }
}
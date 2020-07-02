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
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Common;
using Database;
using Database.Helpers;
using Database.Tables.BasicHouseholds;
using JetBrains.Annotations;

namespace CalculationController.Integrity {
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public static class SimIntegrityChecker {
        private static void CheckSimCategorys([NotNull] Simulator sim, bool cleanup) {
            if (!cleanup) {
                return;
            }
            foreach (var category in sim.Categories) {
                var thisType = category.GetType();
                if (thisType.Name.Contains("CategoryDBBase") || thisType.Name.Contains("CategoryDeviceCategory") ||
                    thisType.Name.Contains("CategorySettlement")) {
                    dynamic d = category;
                    object result = d.CheckForNumbersInNames();
                    if (result != null) {
                        BasicElement db = (BasicElement)result;
                        var bc = (BasicCategory) category;
                        List<BasicElement> bes = new List<BasicElement>();
                        bes.Add(db);
                        throw new DataIntegrityException("The category " + bc.Name + " has the item " + db.Name +
                                                         ", which has a number at the end. This is not pretty. Please fix.", bes);
                    }
                }
            }
        }

        private static void CheckSimDurationValid([NotNull] Simulator sim) {
            var simduration = sim.MyGeneralConfig.EndDateDateTime - sim.MyGeneralConfig.StartDateDateTime;
            var ticks = simduration.Ticks;
            var internalstepticks = sim.MyGeneralConfig.InternalStepSize.Ticks;

            if (ticks % internalstepticks != 0) {
                throw new DataIntegrityException(
                    "The duration of the simulation is not a multiple of the internal step size! It is not possible to simulate for example 1000.5 minutes with an internal step size of 1 minute! This might be caused by for example trying to do a simulation to 31.12.2013 23:59:59. Try instead 01.01.2014 00:00:00.");
            }
        }

        [NotNull]
        [ItemNotNull]
        private static List<BasicChecker> GetAllCheckers(bool isCleanupCheck) {
            var checkers = new List<BasicChecker>
            {
                new DateBasedProfileChecker(isCleanupCheck),
                new AffordanceChecker(isCleanupCheck),
                new DesireChecker(isCleanupCheck),
                new DeviceActionChecker(isCleanupCheck),
                new DeviceActionGroupChecker(isCleanupCheck),
                new DeviceCategoryChecker(isCleanupCheck),
                new DeviceChecker(isCleanupCheck),
                new VacationChecker(isCleanupCheck),
                new DeviceSelectionChecker(isCleanupCheck),
                new DeviceTaggingSetChecker(isCleanupCheck),
                new GeneratorChecker(isCleanupCheck),
                new GeographicLocationChecker(isCleanupCheck),
                new HouseholdTagChecker(isCleanupCheck),
                new HouseChecker(isCleanupCheck),
                new HouseholdTraitChecker(isCleanupCheck),
                new SettlementTemplateChecker(isCleanupCheck),
                new SubAffordanceChecker(isCleanupCheck),
                new PersonChecker(isCleanupCheck),
                new TimeProfileChecker(isCleanupCheck),
                new SettlementChecker(isCleanupCheck),
                new AffordanceTaggingSetChecker(isCleanupCheck),
                new ModularHouseholdChecker(isCleanupCheck),
                new HouseholdPlanChecker(isCleanupCheck),
                new TransformationDeviceChecker(isCleanupCheck),
                new LocationChecker(isCleanupCheck),
                new HouseholdTemplateChecker(isCleanupCheck),
                new TraitTagChecker(isCleanupCheck),
                new HouseTypeChecker(isCleanupCheck),
                new TimeLimitChecker(isCleanupCheck),
                new TravelRouteChecks(isCleanupCheck),
                new TravelRouteSetChecks(isCleanupCheck),
                new SiteChecker(isCleanupCheck)
            };
            return checkers;
        }

        private static void LogProgress(ref DateTime lasttime, ref int step, [NotNull] string name) {
            var now = DateTime.Now;
            Logger
                .Info(
                    "Database integrity check " + step + ":" + name + ": " +
                    (now - lasttime).TotalSeconds.ToString("0.000", CultureInfo.CurrentCulture) + " seconds");
            step++;
            lasttime = now;
        }

        private static void RefreshSubdevicesInDeviceCategories(
            [NotNull][ItemNotNull] ObservableCollection<DeviceCategory> allDeviceCategories) {
            foreach (var allDeviceCategory in allDeviceCategories) {
                allDeviceCategory.RefreshSubDevices();
            }
        }

        public static void Run([NotNull] Simulator sim) {
            var step = 1;
            Logger.Info("Starting the database integrity check");
            var cleanupCheck = sim.MyGeneralConfig.PerformCleanUpChecksBool;
            var start = DateTime.Now;
            CheckSimDurationValid(sim);
            LogProgress(ref start, ref step, "Basic Name Check");
            CheckSimCategorys(sim, cleanupCheck);
            LogProgress(ref start, ref step, "Device Category Refresh");
            RefreshSubdevicesInDeviceCategories(sim.DeviceCategories.Items);

            var checkers = GetAllCheckers(cleanupCheck);
            foreach (var basicChecker in checkers) {
                basicChecker.RunCheck(sim, step++);
            }
        }
    }
}
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CalcPostProcessor.GeneralHouseholdSteps {
    internal class MakeFlexJsonFiles : HouseholdStepBase {

        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public MakeFlexJsonFiles([NotNull] CalcDataRepository repository,
                                 [NotNull] ICalculationProfiler profiler,
                                 [NotNull] IFileFactoryAndTracker fft) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.FlexibilityEvents),
            profiler,
            "Flexibility events",0)
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            var entry = hhp.Key;
            if (entry.KeyType != HouseholdKeyType.Household) {
                return;
            }
            if (!Repository.DoesTableExist(ResultTableID.FlexibilityInformation, entry.HHKey))
            {
                Logger.Error("Found no flexibility data even though it was enabled.");
                return;
            }
            var flexevents =  Repository.LoadFlexibilityEvents(hhp.Key.HHKey);
            var devices = flexevents.Select(x => x.Device.DeviceInstanceGuid).Distinct().ToList();
            Dictionary<string, List<TimeShiftableDeviceActivation>> activationsPerDevice =
                new Dictionary<string, List<TimeShiftableDeviceActivation>>();
            foreach (var guid in devices) {
                activationsPerDevice.Add(guid.StrVal, new List<TimeShiftableDeviceActivation>());
            }

            foreach (var activation in flexevents) {
                activationsPerDevice[activation.Device.DeviceInstanceGuid.StrVal].Add(activation);
            }

            foreach (var activationList in activationsPerDevice.Values) {
                var l = activationList.ToList();
                int maxShifttimeInSteps =(int) ( l[0].Device.MaxTimeShiftInMinutes * 60 / this.Repository.CalcParameters.InternalStepsize.TotalSeconds);
                for (int i = 0; i < l.Count; i++) {
                    int normalstart = l[i].EarliestStart.InternalStep;
                    int lateststart;
                    if (i < l.Count - 1) {
                        lateststart = l[i + 1].EarliestStart.InternalStep - l[i].TotalDuration;
                    }
                    else {
                        lateststart = normalstart + maxShifttimeInSteps;
                    }
                    int diff = lateststart - normalstart;
                    int finaldiff = Math.Min(diff, maxShifttimeInSteps);
                    int finalStep = normalstart + finaldiff;
                    l[i].LatestStart = new TimeStep(finalStep, Repository.CalcParameters);

                }
            }

            var sw =  _fft.MakeFile<StreamWriter>("FlexibilityEvents." + hhp.Key.HHKey.Key + ".json",
                    "Flexibility Events", true, ResultFileID.FlexibilityEventsJson,
                    hhp.Key.HHKey, TargetDirectory.Reports,
                    Repository.CalcParameters.InternalStepsize, CalcOption.LocationsFile);
            sw.WriteLine(JsonConvert.SerializeObject(flexevents,Formatting.Indented) );
            sw.Close();
            var swStats = _fft.MakeFile<StreamWriter>("FlexibilityEventsStatistics." + hhp.Key.HHKey.Key + ".json",
                "Flexibility Events Statistics", true, ResultFileID.FlexibilityEventsStatistics,
                hhp.Key.HHKey, TargetDirectory.Reports,
                Repository.CalcParameters.InternalStepsize, CalcOption.LocationsFile);
            swStats.WriteLine("Total Events:" + flexevents.Count);
            swStats.WriteLine("Total devices:" + devices.Count());
            Dictionary<string, double> energyPerDevice = new Dictionary<string, double>();
            Dictionary<string, string> namePerDevice = new Dictionary<string, string>();
            CalcLoadTypeDto lt = null;
            foreach (var flexevent in flexevents) {
                var elec = flexevent.Profiles.FirstOrDefault(x => x.LoadType.Name == "Electricity");
                if (elec == null) {
                    continue;
                }
                lt = elec.LoadType;
                if (!energyPerDevice.ContainsKey(flexevent.Device.DeviceInstanceGuid.StrVal)) {
                    namePerDevice.Add(flexevent.Device.DeviceInstanceGuid.StrVal, flexevent.Device.Name);
                    energyPerDevice.Add(flexevent.Device.DeviceInstanceGuid.StrVal, 0);
                }
                energyPerDevice[flexevent.Device.DeviceInstanceGuid.StrVal] += elec.Values.Sum();
            }

            if (lt == null) {
                lt = new CalcLoadTypeDto("name", "power", "sum", 1, false, StrGuid.New());
            }
            foreach (var energy in energyPerDevice) {
                string name = namePerDevice[energy.Key];
                swStats.WriteLine(name + "; " + energy.Value * lt.ConversionFactor + " " + lt.UnitOfSum);
            }

            swStats.Close();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();

    }
}
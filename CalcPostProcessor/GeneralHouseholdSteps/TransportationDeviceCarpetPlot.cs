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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using Newtonsoft.Json;

#endregion

namespace CalcPostProcessor.GeneralHouseholdSteps
{
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class TransportationDeviceJson : HouseholdStepBase
    {
            public class JsonStrEntry {
                public JsonStrEntry([JetBrains.Annotations.NotNull] TimeStep time, [CanBeNull] string category)
                {
                    Time = time;
                    Category = category;
                }
                [JetBrains.Annotations.NotNull]
                public TimeStep Time { get; }
                [CanBeNull]
                public string Category { get; }
            }

            public class JsonDoubleEntry
            {
                public JsonDoubleEntry([JetBrains.Annotations.NotNull] TimeStep time, double value)
                {
                    Time = time;
                    Val = value;
                }
                [JetBrains.Annotations.NotNull]
                public TimeStep Time { get; }
                public double Val { get; }
            }



        [JetBrains.Annotations.NotNull] private readonly IFileFactoryAndTracker _fft;

        public TransportationDeviceJson([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
                                        [JetBrains.Annotations.NotNull] CalcDataRepository repository,
                                        [JetBrains.Annotations.NotNull] IFileFactoryAndTracker fft
        ) : base(repository, AutomationUtili.GetOptionList(CalcOption.TansportationDeviceJsons), calculationProfiler,
            "Transportation Device Statistics as Json",10)
        {
            _fft = fft;
        }

        protected override void PerformActualStep([JetBrains.Annotations.NotNull] IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            if (hhp.Key.KeyType != HouseholdKeyType.Household)
            {
                return;
            }

            if (!Repository.CalcParameters.TransportationEnabled)
            {
                return;
            }

            var hhkey = hhp.Key.HHKey;

            ReadActivities(hhkey, out var statebyDevice, out var siteByDevice,
                out var socByDevice, out var drivingDistanceByDevice);
            foreach (var soc in socByDevice) {
                //state of charge
                    JsonSumProfile jsp = new JsonSumProfile("State of Charge for " + soc.Key + " " + hhkey.Key,
                        Repository.CalcParameters.InternalStepsize, Repository.CalcParameters.OfficialStartTime, "State of charge - " + soc.Key, "",
                        null, hhp.Key);
                    foreach (var entry in soc.Value) {
                        if (entry.Time.DisplayThisStep) {
                            jsp.Values.Add(entry.Val);
                        }
                    }

                    var sumfile = _fft.MakeFile<StreamWriter>("Soc." + soc.Key + "." + hhkey.Key + ".json",
                        "SOC Values for " + soc.Key + " in the household " + hhkey.Key, true, ResultFileID.JsonTransportSoc, hhkey,
                        TargetDirectory.Results, Repository.CalcParameters.InternalStepsize, CalcOption.TansportationDeviceJsons, null, null,
                        soc.Key);
                    sumfile.Write(JsonConvert.SerializeObject(jsp, Formatting.Indented));
                    sumfile.Flush();
                }
                foreach (var soc in drivingDistanceByDevice)
                {
                    //driving distance
                    JsonSumProfile jsp = new JsonSumProfile("Driving Distance for " + soc.Key + " " + hhkey.Key,
                        Repository.CalcParameters.InternalStepsize, Repository.CalcParameters.OfficialStartTime, "Driving Distance - " + soc.Key, "",
                        null, hhp.Key);
                    foreach (var entry in soc.Value) {
                        if (entry.Time.DisplayThisStep) {
                            jsp.Values.Add(entry.Val);
                        }
                    }

                    var sumfile = _fft.MakeFile<StreamWriter>("DrivingDistance." + soc.Key + "." + hhkey.Key + ".json",
                        "Driving Distance for " + soc.Key + " in the household " + hhkey.Key, true, ResultFileID.JsonTransportDrivingDistance, hhkey,
                        TargetDirectory.Results, Repository.CalcParameters.InternalStepsize, CalcOption.TansportationDeviceJsons, null, null,
                        soc.Key);
                    sumfile.Write(JsonConvert.SerializeObject(jsp, Formatting.Indented));
                    sumfile.Flush();
                }

                foreach (var soc in statebyDevice)
                {
                    //driving distance
                    JsonEnumProfile jsp = new JsonEnumProfile("Car State for " + soc.Key + " " + hhkey.Key,
                        Repository.CalcParameters.InternalStepsize, Repository.CalcParameters.OfficialStartTime, "Car State - " + soc.Key, "",
                        null, hhp.Key);
                    foreach (var entry in soc.Value)
                    {
                        if (entry.Time.DisplayThisStep)
                        {
                            jsp.Values.Add(entry.Category);
                        }
                    }

                    var sumfile = _fft.MakeFile<StreamWriter>("Carstate." + soc.Key + "." + hhkey.Key + ".json",
                        "Car State for " + soc.Key + " in the household " + hhkey.Key, true, ResultFileID.JsonTransportDeviceState, hhkey,
                        TargetDirectory.Results, Repository.CalcParameters.InternalStepsize, CalcOption.TansportationDeviceJsons, null, null,
                        soc.Key);
                    sumfile.Write(JsonConvert.SerializeObject(jsp, Formatting.Indented));
                    sumfile.Flush();
                }
                foreach (var soc in siteByDevice)
                {
                    //driving distance
                    JsonEnumProfile jsp = new JsonEnumProfile("Car Location for " + soc.Key + " " + hhkey.Key,
                        Repository.CalcParameters.InternalStepsize, Repository.CalcParameters.OfficialStartTime, "Car Location - " + soc.Key, "",
                        null, hhp.Key);
                    foreach (var entry in soc.Value)
                    {
                        if (entry.Time.DisplayThisStep)
                        {
                            jsp.Values.Add(entry.Category);
                        }
                    }

                    var sumfile = _fft.MakeFile<StreamWriter>("CarLocation." + soc.Key + "." + hhkey.Key + ".json",
                        "Car Location for " + soc.Key + " in the household " + hhkey.Key, true, ResultFileID.JsonTransportDeviceLocation, hhkey,
                        TargetDirectory.Results, Repository.CalcParameters.InternalStepsize, CalcOption.TansportationDeviceJsons, null, null,
                        soc.Key);
                    sumfile.Write(JsonConvert.SerializeObject(jsp, Formatting.Indented));
                    sumfile.Flush();
                }
        }

        public override List<CalcOption> NeededOptions { get; } = new List<CalcOption>() {CalcOption.TransportationStatistics};

        private void ReadActivities([JetBrains.Annotations.NotNull] HouseholdKey householdKey,
                                    [JetBrains.Annotations.NotNull] out Dictionary<string, List< JsonStrEntry>> stateByDevice,
                                    [JetBrains.Annotations.NotNull] out Dictionary<string, List< JsonStrEntry>> siteByDevice,
                                    [JetBrains.Annotations.NotNull] out Dictionary<string,List< JsonDoubleEntry>> socByDevice,
        [JetBrains.Annotations.NotNull] out Dictionary<string, List<JsonDoubleEntry>> drivingDistanceByDevice)
        {
            var deviceStates = Repository.LoadTransportationDeviceStates(householdKey);
            stateByDevice = new Dictionary<string, List<JsonStrEntry>>();
            siteByDevice = new Dictionary<string, List<JsonStrEntry>>();
            socByDevice = new Dictionary<string, List<JsonDoubleEntry>>();
            drivingDistanceByDevice = new Dictionary<string, List<JsonDoubleEntry>>();
            foreach (var entry in deviceStates)
            {
                if (!stateByDevice.ContainsKey(entry.TransportationDeviceName))
                {
                    stateByDevice.Add(entry.TransportationDeviceName, new List< JsonStrEntry>());
                    siteByDevice.Add(entry.TransportationDeviceName, new List< JsonStrEntry>());
                    socByDevice.Add(entry.TransportationDeviceName, new List< JsonDoubleEntry>());
                    drivingDistanceByDevice.Add(entry.TransportationDeviceName, new List<JsonDoubleEntry>());
                }
                //state
                JsonStrEntry stateEntry =
                    new JsonStrEntry(entry.TimeStep, entry.TransportationDeviceState);
                stateByDevice[entry.TransportationDeviceName].Add( stateEntry);

                //site
                JsonStrEntry siteEntry =
                    new JsonStrEntry(entry.TimeStep, entry.CurrentSite);
                siteByDevice[entry.TransportationDeviceName].Add( siteEntry);

                //person
                JsonDoubleEntry personEntry =
                    new JsonDoubleEntry(entry.TimeStep, entry.CurrentSOC);
                socByDevice[entry.TransportationDeviceName].Add( personEntry);
                //person
                JsonDoubleEntry drivingDistanceEntry =
                    new JsonDoubleEntry(entry.TimeStep, entry.MovedDistanceInMeters);
                drivingDistanceByDevice[entry.TransportationDeviceName].Add(drivingDistanceEntry);
            }
        }

    }
}
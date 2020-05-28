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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;

#endregion

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class MakeTotalsPerDevice : HouseholdLoadTypeStepBase {

        [NotNull] private readonly IFileFactoryAndTracker _fft;

        private readonly IInputDataLogger _inputDataLogger;

        public MakeTotalsPerDevice([NotNull] CalcDataRepository repository,
                                   [NotNull] ICalculationProfiler profiler,
                                   [NotNull] IFileFactoryAndTracker fft,
                                   IInputDataLogger inputDataLogger) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.TotalsPerDevice),
            profiler,
            "Totals per Device")
        {
            _inputDataLogger = inputDataLogger;
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            var p = (HouseholdLoadtypeStepParameters)parameters;
            if (p.Key.KeyType == HouseholdKeyType.General) {
                return;
            }

            //if (p.Key.KeyType == HouseholdKeyType.House)
            //{
            //return;
            //}
            var deviceActivationEntries = Repository.LoadDeviceActivations(p.Key.HouseholdKey);

            var deviceEnergyDict = new Dictionary<string, double>();
            foreach (var activationEntry in deviceActivationEntries) {
                if (activationEntry.LoadTypeGuid != p.LoadType.Guid) {
                    continue;
                }

                if (!deviceEnergyDict.ContainsKey(activationEntry.DeviceName)) {
                    deviceEnergyDict.Add(activationEntry.DeviceName, 0);
                }

                deviceEnergyDict[activationEntry.DeviceName] += activationEntry.TotalEnergySum;
            }

            var deviceTaggingSetInformations = Repository.GetDeviceTaggingSets();
            var deviceNameToCategory = new Dictionary<string, string>();

            var avgYearlyDict = new Dictionary<CalcLoadTypeDto, Dictionary<StrGuid, double>>();
            if (p.Key.KeyType != HouseholdKeyType.House) {
                avgYearlyDict = GetAverageYearlyConsumptionPerDevice(
                    Repository.LoadDevices(p.Key.HouseholdKey).ConvertAll(x => (ICalcDeviceDto)x));
                var devices = Repository.LoadDevices(p.Key.HouseholdKey);
                foreach (var device in devices) {
                    if (deviceNameToCategory.ContainsKey(device.Name)) {
                        continue;
                    }

                    deviceNameToCategory.Add(device.Name, device.DeviceCategoryName);
                }
            }

            var efc = Repository.ReadEnergyFileColumns(p.Key.HouseholdKey);
            MakeSumsForJson(p.LoadType, p.EnergyFileRows, efc, p.Key.HouseholdKey);
            Run(p.LoadType,
                p.EnergyFileRows,
                _fft,
                efc,
                avgYearlyDict,
                deviceTaggingSetInformations,
                deviceNameToCategory,
                deviceEnergyDict,
                p.Key.HouseholdKey);
            //Repository.DeviceSumInformationList,
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>();

        [NotNull]
        private Dictionary<CalcLoadTypeDto, Dictionary<StrGuid, double>> GetAverageYearlyConsumptionPerDevice(
            [NotNull] [ItemNotNull] List<ICalcDeviceDto> alldevices)
        {
            var averageYearlyConsumptionPerDevice = new Dictionary<CalcLoadTypeDto, Dictionary<StrGuid, double>>();
            // build a device to power dictionary
            foreach (var calcDevice in alldevices) {
                foreach (var calcDeviceLoad in calcDevice.Loads) {
                    var lt = Repository.GetLoadTypeInformationByGuid(calcDeviceLoad.LoadTypeGuid);
                    if (!averageYearlyConsumptionPerDevice.ContainsKey(lt)) {
                        averageYearlyConsumptionPerDevice.Add(lt, new Dictionary<StrGuid, double>());
                    }

                    if (!averageYearlyConsumptionPerDevice[lt].ContainsKey(calcDevice.Guid)) {
                        averageYearlyConsumptionPerDevice[lt].Add(calcDevice.Guid, calcDeviceLoad.AverageYearlyConsumption);
                    }
                }
            }

            return averageYearlyConsumptionPerDevice;
        }

        [NotNull]
        private static Dictionary<StrGuid, Dictionary<int, double>> MakeSumPerMonthPerDeviceID(
            [NotNull] CalcLoadTypeDto dstLoadType,
            [NotNull] EnergyFileColumns efc,
            [NotNull] Dictionary<int, OnlineEnergyFileRow> sumsPerMonth,
            [NotNull] out Dictionary<int, ColumnEntry> columns)
        {
            var sumPerMonthPerDeviceID = new Dictionary<StrGuid, Dictionary<int, double>>();
            columns = efc.ColumnEntriesByColumn[dstLoadType];
            foreach (var onlineEnergyFileRow in sumsPerMonth) {
                var month = onlineEnergyFileRow.Key;

                //var monthsum = onlineEnergyFileRow.Value;
                foreach (var pair in columns) {
                    var ce = pair.Value;
                    if (!sumPerMonthPerDeviceID.ContainsKey(ce.DeviceGuid)) {
                        sumPerMonthPerDeviceID.Add(ce.DeviceGuid, new Dictionary<int, double>());
                    }

                    if (!sumPerMonthPerDeviceID[ce.DeviceGuid].ContainsKey(month)) {
                        sumPerMonthPerDeviceID[ce.DeviceGuid].Add(month, 0);
                    }

                    sumPerMonthPerDeviceID[ce.DeviceGuid][month] += sumsPerMonth[month].EnergyEntries[pair.Key];
                }
            }

            return sumPerMonthPerDeviceID;
        }

        private void MakeSumsForJson([NotNull] CalcLoadTypeDto dstLoadType,
                                     [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                                     [NotNull] EnergyFileColumns efc,
                                     [NotNull] HouseholdKey key)
        {
            var calcParameters = Repository.CalcParameters;
            var dsc = new DateStampCreator(calcParameters);
            if (!efc.ColumnEntriesByColumn.ContainsKey(dstLoadType)) {
                return;
            }

            var cols = efc.ColumnEntriesByColumn[dstLoadType];
            var tdp = new Dictionary<int, TotalsPerDeviceEntry>();
            foreach (var pair in cols) {
                var tpde = new TotalsPerDeviceEntry(key, pair.Value.CalcDeviceDto, dstLoadType);
                tdp.Add(pair.Key, tpde);
            }

            foreach (var row in energyFileRows) {
                if (!row.Timestep.DisplayThisStep) {
                    continue;
                }

                var month = dsc.MakeDateFromTimeStep(row.Timestep).Month;
                foreach (var pair in cols) {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (row.EnergyEntries[pair.Key] == 0) {
                        continue;
                    }

                    tdp[pair.Key].AddConsumptionValue(month, row.EnergyEntries[pair.Key]);
                }
            }

            foreach (var value in tdp.Values) {
                value.Value *= value.Loadtype.ConversionFactor;
                value.NegativeValues *= value.Loadtype.ConversionFactor;
                value.PositiveValues *= value.Loadtype.ConversionFactor;
            }

            _inputDataLogger.SaveList(tdp.Values.ToList().ConvertAll(x => (IHouseholdKey)x));
        }

        [NotNull]
        private Dictionary<int, OnlineEnergyFileRow> MakeSumsPerMonth([NotNull] CalcLoadTypeDto dstLoadType,
                                                                      [ItemNotNull] [NotNull] List<OnlineEnergyFileRow> energyFileRows,
                                                                      DateTime curDate,
                                                                      [NotNull] OnlineEnergyFileRow sum,
                                                                      int rowlength)
        {
            var calcParameters = Repository.CalcParameters;
            var sumsPerMonth = new Dictionary<int, OnlineEnergyFileRow>();
            double runningtotal = 0;
            foreach (var efr in energyFileRows) {
                curDate += calcParameters.InternalStepsize;
                sum.AddValues(efr);
                if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking) {
                    runningtotal += efr.SumFresh();
                }

                if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking && Math.Abs(runningtotal - sum.SumFresh()) > 0.000001) {
                    throw new LPGException("Unknown bug while generating the device totals. Sums don't match.");
                }

                if (!sumsPerMonth.ContainsKey(curDate.Month)) {
                    var ts = new TimeStep(0, 0, true);
                    sumsPerMonth.Add(curDate.Month, new OnlineEnergyFileRow(ts, new List<double>(new double[rowlength]), dstLoadType));
                }

                sumsPerMonth[curDate.Month].AddValues(efr);
            }

            return sumsPerMonth;
        }

        private void MakeTotalsPerDeviceTaggingSet([NotNull] IFileFactoryAndTracker fft,
                                                   [NotNull] CalcLoadTypeDto dstLoadType,
                                                   [ItemNotNull] [NotNull] List<DeviceTaggingSetInformation> deviceTaggingSets,
                                                   [NotNull] Dictionary<string, double> deviceEnergyDict,
                                                   [NotNull] HouseholdKey hhkey)
        {
            var calcParameters = Repository.CalcParameters;
            using (var file = fft.MakeFile<StreamWriter>("DeviceTaggingSet." + dstLoadType.FileName + "." + hhkey.Key + ".csv",
                "Summed up energy use into the AffordanceToCategories in the device tagging sets for " + dstLoadType.Name,
                true,
                ResultFileID.DeviceTaggingSetFiles,
                hhkey,
                TargetDirectory.Reports,
                calcParameters.InternalStepsize,CalcOption.TotalsPerDevice,
                dstLoadType.ConvertToLoadTypeInformation())) {
                foreach (var tagSet in deviceTaggingSets) {
                    file.WriteLine("-----");
                    file.WriteLine(tagSet.Name);
                    var energyUsePerTag = new Dictionary<string, double>();
                    double sum = 0;
                    foreach (var keyValuePair in deviceEnergyDict) {
                        var device = keyValuePair.Key;
                        if (!tagSet.TagByDeviceName.ContainsKey(device)) {
                            tagSet.TagByDeviceName.Add(device, device);
                        }

                        var tag = tagSet.TagByDeviceName[device];
                        if (!energyUsePerTag.ContainsKey(tag)) {
                            energyUsePerTag.Add(tag, 0);
                        }

                        energyUsePerTag[tag] += keyValuePair.Value;
                        sum += keyValuePair.Value;
                    }

                    file.WriteLine("Tag" + calcParameters.CSVCharacter + "Energy Used [" + dstLoadType.UnitOfSum + "]" +
                                   calcParameters.CSVCharacter + "Percentage" + calcParameters.CSVCharacter + "Reference Value" +
                                   calcParameters.CSVCharacter);

                    foreach (var tag in energyUsePerTag) {
                        var referenceValue = string.Empty;
                        var key = DeviceTaggingSetInformation.MakeKey(dstLoadType.Name, tag.Key);
                        if (tagSet.ReferenceValues.ContainsKey(key)) {
                            referenceValue = tagSet.ReferenceValues[key].ToString(Config.CultureInfo);
                        }

                        file.WriteLine(tag.Key + calcParameters.CSVCharacter + tag.Value * dstLoadType.ConversionFactor +
                                       calcParameters.CSVCharacter + (tag.Value / sum).ToString("0.0000", Config.CultureInfo) +
                                       calcParameters.CSVCharacter + referenceValue + calcParameters.CSVCharacter);
                    }

                    file.WriteLine();
                    file.WriteLine("Sum" + calcParameters.CSVCharacter + sum * dstLoadType.ConversionFactor + calcParameters.CSVCharacter + "1" +
                                   calcParameters.CSVCharacter);
                }
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private void Run([NotNull] CalcLoadTypeDto dstLoadType,
                         [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                         [NotNull] IFileFactoryAndTracker fft,
                         [NotNull] EnergyFileColumns efc,
                         [NotNull] Dictionary<CalcLoadTypeDto, Dictionary<StrGuid, double>> loadTypeTodeviceIDToAverageLookup,
                         [ItemNotNull] [NotNull] List<DeviceTaggingSetInformation> deviceTaggingSets,
                         [NotNull] Dictionary<string, string> deviceNameToCategory,
                         [NotNull] Dictionary<string, double> deviceEnergyDict,
                         [NotNull] HouseholdKey key)
        {
            if (!efc.ColumnEntriesByColumn.ContainsKey(dstLoadType)) {
                //for this load type for this house there are no column, so nothing to do
                return;
            }
            var calcParameters = Repository.CalcParameters;
            var rowlength = energyFileRows[0].EnergyEntries.Count;
            var ts = new TimeStep(0, 0, true);
            var sum = new OnlineEnergyFileRow(ts, new List<double>(new double[rowlength]), dstLoadType);
            var curDate = calcParameters.OfficialStartTime;
            var sumsPerMonth = MakeSumsPerMonth(dstLoadType, energyFileRows, curDate, sum, rowlength);
            /*
            if (Config.IsInUnitTesting && Config.ExtraUnitTestChecking) {
                if (!double.IsNaN(previousTotal) && Math.Abs(sum.SumFresh - previousTotal) > 0.000001) {
                    throw new LPGException("Unknown bug while generating the device totals. Sums don't match.");
                }
            }*/
            var sumPerMonthPerDeviceID = MakeSumPerMonthPerDeviceID(dstLoadType, efc, sumsPerMonth, out var columns);

            var sumsPerDeviceID = new Dictionary<StrGuid, double>();
            var deviceNamesPerID = new Dictionary<StrGuid, string>();
            var sumPerDeviceName = new Dictionary<string, double>();
            foreach (var pair in columns) {
                var ce = pair.Value;
                if (!sumsPerDeviceID.ContainsKey(ce.DeviceGuid)) {
                    sumsPerDeviceID.Add(ce.DeviceGuid, 0);
                    deviceNamesPerID.Add(ce.DeviceGuid, ce.Name);
                }

                if (!sumPerDeviceName.ContainsKey(ce.Name)) {
                    sumPerDeviceName.Add(ce.Name, 0);
                }

                sumPerDeviceName[ce.Name] += sum.EnergyEntries[pair.Key];
                sumsPerDeviceID[ce.DeviceGuid] += sum.EnergyEntries[pair.Key];
            }

            MakeTotalsPerDeviceTaggingSet(fft, dstLoadType, deviceTaggingSets, deviceEnergyDict, key);
            var builder = new StringBuilder();
            foreach (var calcDeviceTaggingSet in deviceTaggingSets) {
                if (calcDeviceTaggingSet.LoadTypesForThisSet.Any(x => x.Name == dstLoadType.Name)) {
                    builder.Append(calcDeviceTaggingSet.Name).Append(calcParameters.CSVCharacter);
                }
            }

            var taggingsetHeader = builder.ToString();
            var devicesums = fft.MakeFile<StreamWriter>("DeviceSums." + dstLoadType.Name + "." + key.Key + ".csv",
                "Summed up " + dstLoadType.Name + " use per device and comparison with statistical values",
                true,
                ResultFileID.DeviceSums,
                key,
                TargetDirectory.Reports,
                calcParameters.InternalStepsize,CalcOption.TotalsPerDevice,
                dstLoadType.ConvertToLoadTypeInformation());
            var calcDuration = calcParameters.OfficialEndTime - calcParameters.OfficialStartTime;
            var amountofYears = calcDuration.TotalDays / 365.0;
            var sb = new StringBuilder();
            sb.Append("Device name");
            sb.Append(calcParameters.CSVCharacter);
            sb.Append("Usage sum in this simulation [").Append(dstLoadType.UnitOfSum).Append("]");
            sb.Append(calcParameters.CSVCharacter);
            sb.Append("Usage sum in this simulation linear extrapolated to 1 year [").Append(dstLoadType.UnitOfSum).Append("]");
            sb.Append(calcParameters.CSVCharacter);

            sb.Append("Comparison Value from the device entry [").Append(dstLoadType.UnitOfSum).Append("]");
            sb.Append(calcParameters.CSVCharacter);
            sb.Append("Percentage of the comparison value [1 = 100%]");
            sb.Append(calcParameters.CSVCharacter);
            sb.Append("Device Category");
            sb.Append(calcParameters.CSVCharacter);
            sb.Append(taggingsetHeader);

            devicesums.WriteLine(sb);
            double devicesum = 0;
            double extrapolatedSum = 0;
            double comparsionvaluessum = 0;

            foreach (var keyValuePair in sumsPerDeviceID) {
                var s = string.Empty;
                s += deviceNamesPerID[keyValuePair.Key];

                s += calcParameters.CSVCharacter;
                s += keyValuePair.Value * dstLoadType.ConversionFactor;
                devicesum += keyValuePair.Value;

                //deviceSums.AddDeviceSum(deviceNamesPerID[keyValuePair.Key],devicesum,dstLoadType);

                s += calcParameters.CSVCharacter;
                var extrapolatedValue = keyValuePair.Value * dstLoadType.ConversionFactor / amountofYears;
                s += extrapolatedValue;
                extrapolatedSum += keyValuePair.Value / amountofYears;

                s += calcParameters.CSVCharacter;
                double defaultvalue = 0;
                if (loadTypeTodeviceIDToAverageLookup.ContainsKey(dstLoadType)) {
                    if (loadTypeTodeviceIDToAverageLookup[dstLoadType].ContainsKey(keyValuePair.Key)) {
                        defaultvalue = loadTypeTodeviceIDToAverageLookup[dstLoadType][keyValuePair.Key];
                    }
                }

                s += defaultvalue;
                comparsionvaluessum += defaultvalue;
                s += calcParameters.CSVCharacter;
                if (Math.Abs(defaultvalue) > Constants.Ebsilon) {
                    s += extrapolatedValue / defaultvalue;
                }
                else {
                    s += 0;
                }

                s += calcParameters.CSVCharacter;
                var devicename = deviceNamesPerID[keyValuePair.Key];
                var deviceCategory = "(no category)";
                if (deviceNameToCategory.ContainsKey(devicename)) {
                    deviceCategory = deviceNameToCategory[devicename];
                }

                s += deviceCategory;
                s += calcParameters.CSVCharacter;
                var tags = string.Empty;
                foreach (var calcDeviceTaggingSet in deviceTaggingSets) {
                    if (calcDeviceTaggingSet.LoadTypesForThisSet.Any(x => x.Name == dstLoadType.Name)) {
                        var deviceName = deviceNamesPerID[keyValuePair.Key];
                        if (calcDeviceTaggingSet.TagByDeviceName.ContainsKey(deviceName)) {
                            tags += calcDeviceTaggingSet.TagByDeviceName[deviceName] + calcParameters.CSVCharacter;
                        }
                        else {
                            tags += Constants.UnknownTag + calcParameters.CSVCharacter;
                        }
                    }
                }

                devicesums.WriteLine(s + tags);
            }

            var sumstr = "Sums";
            sumstr += calcParameters.CSVCharacter;
            sumstr += devicesum * dstLoadType.ConversionFactor;
            sumstr += calcParameters.CSVCharacter;
            sumstr += extrapolatedSum * dstLoadType.ConversionFactor;
            sumstr += calcParameters.CSVCharacter;
            sumstr += comparsionvaluessum;
            devicesums.WriteLine(sumstr);
            devicesums.Flush();
            WriteMonthlyDeviceSums(fft, dstLoadType, sumPerMonthPerDeviceID, deviceNamesPerID, key);
        }

        private void WriteMonthlyDeviceSums([NotNull] IFileFactoryAndTracker fft,
                                            [NotNull] CalcLoadTypeDto dstLoadType,
                                            [NotNull] Dictionary<StrGuid, Dictionary<int, double>> values,
                                            [NotNull] Dictionary<StrGuid, string> deviceNamesPerID,
                                            [NotNull] HouseholdKey key)
        {
            var calcParameters = Repository.CalcParameters;
            var devicesums = fft.MakeFile<StreamWriter>("DeviceSums_Monthly." + dstLoadType.Name + "." + key.Key + ".csv",
                "Summed up " + dstLoadType.Name + " use per device per Month",
                true,
                ResultFileID.DeviceSumsPerMonth,
                key,
                TargetDirectory.Reports,
                calcParameters.InternalStepsize,CalcOption.TotalsPerDevice,
                dstLoadType.ConvertToLoadTypeInformation());

            var sb = new StringBuilder();
            sb.Append("Device name");
            sb.Append(calcParameters.CSVCharacter);
            var firstmonth = values.Keys.First();
            foreach (var monthAndValue in values[firstmonth]) {
                sb.Append(monthAndValue.Key);
                sb.Append(calcParameters.CSVCharacter);
            }

            devicesums.WriteLine(sb);
            var monthSums = new Dictionary<int, double>();

            foreach (var keyValuePair in values) {
                var s = string.Empty;
                s += deviceNamesPerID[keyValuePair.Key];
                s += calcParameters.CSVCharacter;

                foreach (var monthValue in keyValuePair.Value) {
                    s += monthValue.Value * dstLoadType.ConversionFactor + calcParameters.CSVCharacter;
                    if (!monthSums.ContainsKey(monthValue.Key)) {
                        monthSums.Add(monthValue.Key, 0);
                    }

                    monthSums[monthValue.Key] += monthValue.Value;
                }

                devicesums.WriteLine(s);
            }

            var sumstr = "Sums" + calcParameters.CSVCharacter;
            foreach (var keyValuePair in monthSums) {
                sumstr += keyValuePair.Value * dstLoadType.ConversionFactor + calcParameters.CSVCharacter;
            }

            devicesums.WriteLine(sumstr);
            devicesums.Flush();
        }
    }
}
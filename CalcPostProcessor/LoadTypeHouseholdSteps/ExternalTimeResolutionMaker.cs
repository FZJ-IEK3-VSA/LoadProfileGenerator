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
using System.Globalization;
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
using JetBrains.Annotations;
using Newtonsoft.Json;

#endregion

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class ExternalTimeResolutionMaker : LoadTypeStepBase {


        [NotNull] private readonly IFileFactoryAndTracker _fft;

        public ExternalTimeResolutionMaker([NotNull] IFileFactoryAndTracker fft,
                                           [NotNull] CalcDataRepository repository,
                                           [NotNull] ICalculationProfiler calculationProfiler) : base(repository,
            AutomationUtili.GetOptionList(CalcOption.SumProfileExternalEntireHouse,CalcOption.SumProfileExternalIndividualHouseholds,
                CalcOption.DeviceProfileExternalEntireHouse,CalcOption.DeviceProfileExternalIndividualHouseholds,
                CalcOption.SumProfileExternalIndividualHouseholdsAsJson), calculationProfiler, "External Time Resolution Profiles")
        {
            _fft = fft;
        }

        public void Run([NotNull] CalcLoadTypeDto dstLoadType,
                        [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                        [NotNull] EnergyFileColumns efc)
        {
            var calcParameters = Repository.CalcParameters;
            var dsc = new DateStampCreator(calcParameters);
            var externalfactor =
                (int)
                (calcParameters.ExternalStepsize.TotalSeconds /
                 calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }

            var externalFileName =
                calcParameters.ExternalStepsize.TotalSeconds.ToString(CultureInfo.InvariantCulture);

            StreamWriter sumfile = null;
            if (calcParameters.IsSet(CalcOption.SumProfileExternalEntireHouse)) {
                sumfile =
                    _fft.MakeFile<StreamWriter>("SumProfiles_" + externalFileName + "s." + dstLoadType.Name + ".csv",
                        "Sum energy profiles for " + externalFileName + "s " + dstLoadType.Name, true,
                        ResultFileID.CSVSumProfileExternal, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                        calcParameters.InternalStepsize,CalcOption.SumProfileExternalEntireHouse,
                        dstLoadType.ConvertToLoadTypeInformation());
                sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" +
                                  dstLoadType.UnitOfSum + "]");
            }

            StreamWriter normalfile = null;
            if (calcParameters.IsSet(CalcOption.DeviceProfileExternalEntireHouse)) {
                normalfile =
                    _fft.MakeFile<StreamWriter>("DeviceProfiles_" + externalFileName + "s." + dstLoadType.Name + ".csv",
                        "Device energy profiles for " + externalFileName + "s " + dstLoadType.Name, true,
                        ResultFileID.DeviceProfileCSVExternal, Constants.GeneralHouseholdKey, TargetDirectory.Results,
                        calcParameters.InternalStepsize,CalcOption.DeviceProfileExternalEntireHouse,
                        dstLoadType.ConvertToLoadTypeInformation());
                normalfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() +
                                     efc.GetTotalHeaderString(dstLoadType, null));
            }

            if (calcParameters.IsSet(CalcOption.DeviceProfileExternalEntireHouse) ||
                calcParameters.IsSet(CalcOption.SumProfileExternalEntireHouse)) {
                for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                    var efr = new OnlineEnergyFileRow(energyFileRows[outerIndex]);
                    if (!efr.Timestep.DisplayThisStep) {
                        continue;
                    }

                    for (var innerIndex = outerIndex + 1;
                        innerIndex < externalfactor + outerIndex && innerIndex < energyFileRows.Count;
                        innerIndex++) {
                        var efr2 = energyFileRows[innerIndex];
                        efr.AddValues(efr2);
                    }

                    var sb = new StringBuilder();
                    dsc.GenerateDateStampForTimestep(efr.Timestep, sb);
                    if (calcParameters.IsSet(CalcOption.DeviceProfileExternalEntireHouse)) {
                        var normalstr =
                            sb + efr.GetEnergyEntriesAsString(true, dstLoadType, null, calcParameters.CSVCharacter,
                                    calcParameters.DecimalSeperator)
                                .ToString();
                        if (normalfile == null) {
                            throw new LPGException("File is null. Please report.");
                        }

                        normalfile.WriteLine(normalstr);
                    }

                    if (calcParameters.IsSet(CalcOption.SumProfileExternalEntireHouse)) {
                        if (sumfile == null) {
                            throw new LPGException("File is null. Please report.");
                        }

                        sumfile.WriteLine(sb +
                                          (efr.SumCached * dstLoadType.ConversionFactor).ToString(Config.CultureInfo));
                    }
                }
            }
        }

        public void RunIndividualHouseholds([NotNull] CalcLoadTypeDto dstLoadType,
                                            [NotNull] [ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
                                            [NotNull] EnergyFileColumns efc,
                                            [NotNull] HouseholdKey hhnum)
        {
            var calcParameters = Repository.CalcParameters;
            var dsc = new DateStampCreator(calcParameters);
            var externalfactor =
                (int)
                (calcParameters.ExternalStepsize.TotalSeconds /
                 calcParameters.InternalStepsize.TotalSeconds);
            if (externalfactor == 1) {
                return;
            }

            var externalFileName =
                calcParameters.ExternalStepsize.TotalSeconds.ToString(CultureInfo.InvariantCulture);
            var columns =
                (from entry in efc.ColumnEntriesByColumn[dstLoadType].Values
                    where entry.HouseholdKey == hhnum
                    select entry.Column).ToList();
            var hhname = "." + hhnum + ".";
            StreamWriter sumfile = null;
            if (calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholds)) {
                sumfile =
                    _fft.MakeFile<StreamWriter>(
                        "SumProfiles_" + externalFileName + "s" + hhname + dstLoadType.Name + ".csv",
                        "Summed up energy profile for all devices for " + dstLoadType.Name + " for " + hhname +
                        " for " + externalFileName + "s", true,
                        ResultFileID.ExternalSumsForHouseholds, hhnum,
                        TargetDirectory.Results, calcParameters.ExternalStepsize, CalcOption.SumProfileExternalIndividualHouseholds,
                        dstLoadType.ConvertToLoadTypeInformation());
                sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() +
                                  "Sum [" +
                                  dstLoadType.UnitOfSum + "]");
            }

            StreamWriter normalfile = null;
            if (calcParameters.IsSet(CalcOption.DeviceProfileExternalIndividualHouseholds)) {
                normalfile =
                    _fft.MakeFile<StreamWriter>(
                        "DeviceProfiles_" + externalFileName + "s" + hhname + dstLoadType.Name + ".csv",
                        "Energy use by each device in each Timestep for " + dstLoadType.Name + " for " + hhname,
                        true, ResultFileID.DeviceProfileCSVExternalForHouseholds, hhnum,
                        TargetDirectory.Results, calcParameters.ExternalStepsize,CalcOption.DeviceProfileExternalIndividualHouseholds,
                        dstLoadType.ConvertToLoadTypeInformation() );
                normalfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() +
                                     efc.GetTotalHeaderString(dstLoadType, columns));
            }
            StreamWriter jsonfile = null;
            List<double> valuesForJsonExport = new List<double>();
            if (calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson))
            {
                jsonfile =
                    _fft.MakeFile<StreamWriter>(
                        "SumProfiles_" + externalFileName + "s" + hhname + dstLoadType.Name + ".json",
                        "Summed up energy profile for all devices for " + dstLoadType.Name + " for " + hhname +
                        " for " + externalFileName + "s as json", true,
                        ResultFileID.ExternalSumsForHouseholdsJson, hhnum,
                        TargetDirectory.Results, calcParameters.ExternalStepsize,CalcOption.SumProfileExternalIndividualHouseholdsAsJson,
                        dstLoadType.ConvertToLoadTypeInformation());
            }

            if (calcParameters.IsSet(CalcOption.DeviceProfileExternalIndividualHouseholds) ||
                calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholds) ||
                calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson)) {
                for (var outerIndex = 0; outerIndex < energyFileRows.Count; outerIndex += externalfactor) {
                    var efr = new OnlineEnergyFileRow(energyFileRows[outerIndex]);
                    if (!efr.Timestep.DisplayThisStep) {
                        continue;
                    }

                    for (var innerIndex = outerIndex + 1;
                        innerIndex < externalfactor + outerIndex && innerIndex < energyFileRows.Count;
                        innerIndex++) {
                        var efr2 = energyFileRows[innerIndex];
                        efr.AddValues(efr2);
                    }

                    var sb = new StringBuilder();
                    dsc.GenerateDateStampForTimestep(efr.Timestep, sb);
                    if (calcParameters.IsSet(CalcOption.DeviceProfileExternalIndividualHouseholds)) {
                        var normalstr = sb.ToString() +
                                        efr.GetEnergyEntriesAsString(true, dstLoadType, columns,
                                            calcParameters.CSVCharacter, calcParameters.DecimalSeperator);
                        if (normalfile == null) {
                            throw new LPGException("File was null. Please report.");
                        }

                        normalfile.WriteLine(normalstr);
                    }

                    if (calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholds)||
                        calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson)) {
                        double sum = efr.GetSumForCertainCols(columns) * dstLoadType.ConversionFactor;
                        if (calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholds)) {
                            var sumstring = sb.ToString() + sum;
                            if (sumfile == null) {
                                throw new LPGException("File was null. Please report.");
                            }
                            sumfile.WriteLine(sumstring);
                        }
                        if(calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson)) {
                            valuesForJsonExport.Add(sum);
                        }
                    }
                }

                if (calcParameters.IsSet(CalcOption.SumProfileExternalIndividualHouseholdsAsJson)) {
                    if (jsonfile == null) {
                        throw new LPGException("Jsonfile was null");
                    }
                    jsonfile.WriteLine(JsonConvert.SerializeObject(valuesForJsonExport,Formatting.Indented));
                }
            }
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            Run(p.LoadType, p.EnergyFileRows, efc);
            foreach (HouseholdKeyEntry key in Repository.HouseholdKeys) {
                RunIndividualHouseholds(p.LoadType, p.EnergyFileRows, efc, key.HHKey);
            }
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.DetailedDatFiles
        };
    }
}
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

using System.Collections.Generic;
using System.IO;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeHouseholdSteps {
    public class DeviceProfileFileProcessor: LoadTypeStepBase
    {
        [NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public DeviceProfileFileProcessor([NotNull] IFileFactoryAndTracker fft,
                                          [NotNull] CalcDataRepository repository,
            [NotNull] ICalculationProfiler profiler):base(repository, AutomationUtili.GetOptionList(CalcOption.DeviceProfiles, CalcOption.IndividualSumProfiles),profiler,"Device Profiles")
        {
            _fft = fft;
        }

        private void Run([NotNull] CalcLoadTypeDto dstLoadType, [NotNull][ItemNotNull] List<OnlineEnergyFileRow> energyFileRows,
              [NotNull] HouseholdKey householdKey, [NotNull] EnergyFileColumns efc, DateStampCreator dsc)
        {
            var calcParameters = Repository.CalcParameters;
            StreamWriter sumfile = null;
            if (calcParameters.IsSet(CalcOption.IndividualSumProfiles)) {
                sumfile = _fft.MakeFile<StreamWriter>("SumProfiles." + dstLoadType.Name + ".csv",
                    "Summed up energy profile for all devices for " + dstLoadType.Name, true,
                    ResultFileID.CSVSumProfile, householdKey, TargetDirectory.Results,
                    calcParameters.InternalStepsize,CalcOption.IndividualSumProfiles,
                    dstLoadType.ConvertToLoadTypeInformation());
                sumfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() + "Sum [" +
                                  dstLoadType.UnitOfSum + "]");
            }
            StreamWriter normalfile = null;
            if (calcParameters.IsSet(CalcOption.DeviceProfiles)) {
                normalfile = _fft.MakeFile<StreamWriter>("DeviceProfiles." + dstLoadType.Name + ".csv",
                    "Energy use by each device in each Timestep for " + dstLoadType.Name, true,
                    ResultFileID.DeviceProfileCSV, householdKey, TargetDirectory.Results,
                    calcParameters.InternalStepsize,CalcOption.DeviceProfiles,
                    dstLoadType.ConvertToLoadTypeInformation());
                normalfile.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() +
                                     efc.GetTotalHeaderString(dstLoadType, null));
            }
            if (calcParameters.IsSet(CalcOption.IndividualSumProfiles) ||
                calcParameters.IsSet(CalcOption.DeviceProfiles)) {
                foreach (var efr in energyFileRows) {
                    if(!efr.Timestep.DisplayThisStep) {
                        continue;
                    }

                    var time = dsc.MakeTimeString(efr.Timestep);
                    if (calcParameters.IsSet(CalcOption.DeviceProfiles)) {
                        var individual = time + efr.GetEnergyEntriesAsString(true, dstLoadType, null,calcParameters.CSVCharacter);
                        if (normalfile == null) {
                            throw new LPGException("File is null, even though it shouldn't be. Please report.");
                        }
                        normalfile.WriteLine(individual);
                    }
                    if (calcParameters.IsSet(CalcOption.IndividualSumProfiles)) {
                        var sumstring =
                            time + (efr.SumCached * dstLoadType.ConversionFactor).ToString(Config.CultureInfo);
                        if (sumfile == null) {
                            throw new LPGException("File is null, even though it shouldn't be. Please report.");
                        }
                        sumfile.WriteLine(sumstring);
                    }
                }
            }
            if (calcParameters.IsSet(CalcOption.DeviceProfiles)) {
                if (normalfile == null) {
                    throw new LPGException("File is null, even though it shouldn't be. Please report.");
                }
                normalfile.Flush();
            }
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var dsc = new DateStampCreator(Repository.CalcParameters);
            //TODO: check: is this correcT? only general household profiles?
            Run( p.LoadType,
                p.EnergyFileRows, Constants.GeneralHouseholdKey,
                Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey), dsc);
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.DetailedDatFiles
        };
    }
}
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
using Common.SQLResultLogging;
using JetBrains.Annotations;

namespace CalcPostProcessor.LoadTypeProcessingSteps {
    public class HouseDeviceProfileFileProcessor: LoadTypeStepBase
    {
        [NotNull]
        private readonly IFileFactoryAndTracker _fft;

        public HouseDeviceProfileFileProcessor([NotNull] IFileFactoryAndTracker fft,
                                          [NotNull] CalcDataRepository repository,
            [NotNull] ICalculationProfiler profiler):base(repository, AutomationUtili.GetOptionList(CalcOption.DeviceProfilesHouse),
            profiler,"House Device Profiles")
        {
            _fft = fft;
        }

        protected override void PerformActualStep(IStepParameters parameters)
        {
            LoadtypeStepParameters p = (LoadtypeStepParameters)parameters;
            var dsc = new DateStampCreator(Repository.CalcParameters);
            var dstLoadType = p.LoadType;
            var householdKey = Constants.GeneralHouseholdKey;
            var calcParameters = Repository.CalcParameters;
            var efc = Repository.ReadEnergyFileColumns(Constants.GeneralHouseholdKey);
            var deviceProfileCsv = _fft.MakeFile<StreamWriter>("DeviceProfiles." + dstLoadType.Name + ".csv",
                "Energy use by each device in each Timestep for " + dstLoadType.Name+  " for the entire house", true,
                ResultFileID.DeviceProfileCSV, householdKey, TargetDirectory.Results,
                calcParameters.InternalStepsize, CalcOption.DeviceProfilesHouse,
                dstLoadType.ConvertToLoadTypeInformation());
            deviceProfileCsv.WriteLine(dstLoadType.Name + "." + dsc.GenerateDateStampHeader() +
                                       efc.GetTotalHeaderString(dstLoadType, null));
            foreach (var efr in p.EnergyFileRows)
            {
                if (!efr.Timestep.DisplayThisStep)
                {
                    continue;
                }

                var time = dsc.MakeTimeString(efr.Timestep);
                    var individual = time + efr.GetEnergyEntriesAsString(true, dstLoadType, null, calcParameters.CSVCharacter, calcParameters.DecimalSeperator);
                    deviceProfileCsv.WriteLine(individual);
            }
            deviceProfileCsv.Flush();
        }

        [NotNull]
        public override List<CalcOption> NeededOptions => new List<CalcOption>() {
            CalcOption.DetailedDatFiles
        };
    }
}
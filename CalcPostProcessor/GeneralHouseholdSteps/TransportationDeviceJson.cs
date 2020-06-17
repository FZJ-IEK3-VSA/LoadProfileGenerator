////-----------------------------------------------------------------------

//// <copyright>
////
//// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
//// Written by Noah Pflugradt.
//// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
////
//// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
////  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
//// in the documentation and/or other materials provided with the distribution.
////  All advertising materials mentioning features or use of this software must display the following acknowledgement:
////  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
////  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
////  derived from this software without specific prior written permission.
////
//// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
//// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
//// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
//// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
//// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

//// </copyright>

////-----------------------------------------------------------------------

//#region

//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using Automation;
//using Automation.ResultFiles;
//using CalcPostProcessor.Steps;
//using Common;
//using Common.JSON;
//using Common.SQLResultLogging;
//using JetBrains.Annotations;
//using Color = System.Windows.Media.Color;

//#endregion

//namespace CalcPostProcessor.GeneralHouseholdSteps {
//    public class TransportationDeviceCarpetPlot : HouseholdStepBase {
//        [JetBrains.Annotations.NotNull] private readonly CalcParameters _calcParameters;

//        [JetBrains.Annotations.NotNull] private readonly FileFactoryAndTracker _fft;

//        public TransportationDeviceCarpetPlot([JetBrains.Annotations.NotNull] ICalculationProfiler calculationProfiler,
//                                [JetBrains.Annotations.NotNull] CalcDataRepository repository,
//                                [JetBrains.Annotations.NotNull] FileFactoryAndTracker fft
//        ) : base(repository, AutomationUtili.GetOptionList(CalcOption.TransportationDeviceCarpetPlot, CalcOption.TransportationStatistics), calculationProfiler,
//            "Transportation Device Carpet Plot")
//        {
//            _calcParameters = Repository.CalcParameters;
//            _fft = fft;
//        }

//        [JetBrains.Annotations.NotNull] private readonly ColourGenerator _cg = new ColourGenerator();
//        protected override void PerformActualStep([JetBrains.Annotations.NotNull] IStepParameters parameters)
//        {
//            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
//            if (hhp.Key.KeyType != HouseholdKeyType.Household) {
//                return;
//            }

//            if (!_calcParameters.IsInTranportMode(hhp.Key.HouseholdKey)) {
//                return;
//            }

//            var hhkey = hhp.Key.HouseholdKey;

//            TimeSpan officialSimulationDuration = _calcParameters.OfficialEndTime - _calcParameters.OfficialStartTime;
//            CategoryCarpetPlotMaker ccpm = new CategoryCarpetPlotMaker(_calcParameters);
//            ReadActivities(hhkey,out var statebyDevice, out var siteByDevice, out var personByDevice);
//            int chartindex = 1;
//            foreach (var pair in statebyDevice) {
//                string fileName = "TransportationStateCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key + ".png" ;
//                 string legendFilename = "TransportationStateCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key + ".Legend.png";
//                const string filedescription = "";
//                 const string legendDescription = "";
//                ccpm.MakeCarpet(_fft, pair.Value, officialSimulationDuration, hhp.Key.HouseholdKey,
//                    Repository.CarpetPlotColumnWidth, fileName, filedescription,
//                    1, legendFilename, legendDescription, "TransportationStateCarpetPlot:" + chartindex.ToString());
//                chartindex++;
//            }

//            chartindex = 1;
//            foreach (KeyValuePair<string, Dictionary<int, CarpetCategoryEntry>> pair in siteByDevice) {
//                string fileName = "TransportationDeviceSiteCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key + ".png";
//                string legendFilename = "TransportationDeviceSiteCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key +
//                                        ".Legend.png";
//                const string filedescription = "";
//                const string legendDescription = "";
//                ccpm.MakeCarpet(_fft, pair.Value, officialSimulationDuration, hhp.Key.HouseholdKey,
//                    Repository.CarpetPlotColumnWidth, fileName, filedescription, 1,
//                    legendFilename, legendDescription,
//                    "TransportationDeviceSiteCarpetPlot:"+chartindex.ToString());
//                chartindex++;
//            }

//            foreach (KeyValuePair<string, Dictionary<int, CarpetCategoryEntry>> pair in personByDevice)
//            {
//                string fileName = "TransportationDeviceUserCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key + ".png";
//                string legendFilename = "TransportationDeviceUserCarpetPlot." + hhp.Key.HouseholdKey.Key + "." + pair.Key +
//                                        ".Legend.png";
//                const string filedescription = "";
//                const string legendDescription = "";
//                ccpm.MakeCarpet(_fft, pair.Value, officialSimulationDuration, hhp.Key.HouseholdKey,
//                    Repository.CarpetPlotColumnWidth, fileName, filedescription, 1, legendFilename, legendDescription,
//                   "TransportationDeviceUserCarpetPlot:"+ chartindex.ToString());
//                chartindex++;
//            }
//        }

//        private void ReadActivities([JetBrains.Annotations.NotNull] HouseholdKey householdKey,
//                                    [JetBrains.Annotations.NotNull] out Dictionary<string, Dictionary<int, CarpetCategoryEntry>> stateByDevice,
//                                    [JetBrains.Annotations.NotNull] out Dictionary<string, Dictionary<int, CarpetCategoryEntry>> siteByDevice,
//                                    [JetBrains.Annotations.NotNull] out Dictionary<string, Dictionary<int, CarpetCategoryEntry>> userByDevice)
//        {
//            var deviceStates = Repository.LoadTransportationDeviceStates(householdKey);
//            stateByDevice = new  Dictionary<string, Dictionary<int, CarpetCategoryEntry>>();
//            siteByDevice = new Dictionary<string, Dictionary<int, CarpetCategoryEntry>>();
//            userByDevice = new Dictionary<string, Dictionary<int, CarpetCategoryEntry>>();
//            Dictionary<string, Color> stateColors = new Dictionary<string, Color>();
//            Dictionary<string, Color> siteColors = new Dictionary<string, Color>();
//            Dictionary<string, Color> personColors = new Dictionary<string, Color>();
//            foreach (var entry in deviceStates) {
//                if (!stateByDevice.ContainsKey(entry.TransportationDeviceName)) {
//                    stateByDevice.Add(entry.TransportationDeviceName, new Dictionary<int, CarpetCategoryEntry>());
//                    siteByDevice.Add(entry.TransportationDeviceName, new Dictionary<int, CarpetCategoryEntry>());
//                    userByDevice.Add(entry.TransportationDeviceName, new Dictionary<int, CarpetCategoryEntry>());
//                }
//                //state
//                Color locColor = GetColor(entry.TransportationDeviceState, stateColors);
//                CarpetCategoryEntry stateEntry =
//                    new CarpetCategoryEntry(entry.TimeStep, entry.TransportationDeviceState, locColor, false);
//                stateByDevice[entry.TransportationDeviceName].Add(entry.TimeStep.InternalStep, stateEntry);

//                //site
//                    Color siteColor = GetColor(entry.CurrentSite, siteColors);
//                    CarpetCategoryEntry siteEntry =
//                        new CarpetCategoryEntry(entry.TimeStep, entry.CurrentSite, siteColor, false);
//                siteByDevice[entry.TransportationDeviceName].Add(entry.TimeStep.InternalStep, siteEntry);

//                //person
//                Color personColor = GetColor(entry.CurrentUser, personColors);
//                CarpetCategoryEntry personEntry =
//                    new CarpetCategoryEntry(entry.TimeStep, entry.CurrentUser, personColor, false);
//                userByDevice[entry.TransportationDeviceName].Add(entry.TimeStep.InternalStep, personEntry);
//            }
//        }

//        private Color GetColor([CanBeNull] string key, [JetBrains.Annotations.NotNull] Dictionary<string, Color> colors)
//        {
//            if (key == null) {
//                return Color.FromRgb(0, 0, 0);
//            }

//            if (!colors.ContainsKey(key)) {
//                string htmlc = _cg.NextColour();
//                var c1 = ColorTranslator.FromHtml("#"+htmlc);
//                Color c = Color.FromRgb(c1.R,c1.G,c1.B);
//                colors.Add(key,c);
//                return c;
//            }
//            return colors[key];
//        }
//    }
//}
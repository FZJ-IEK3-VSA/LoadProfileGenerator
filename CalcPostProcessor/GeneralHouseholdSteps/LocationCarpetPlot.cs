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
using System.Drawing;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using CalcPostProcessor.Steps;
using Common;
using Common.CalcDto;
using Common.JSON;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using Color = System.Windows.Media.Color;

#endregion

namespace CalcPostProcessor.GeneralHouseholdSteps {
    public class LocationCarpetPlot : HouseholdStepBase {
        [NotNull] private readonly CalcParameters _calcParameters;

        [NotNull] private readonly FileFactoryAndTracker _fft;

        public LocationCarpetPlot([NotNull] ICalculationProfiler calculationProfiler,
                                [NotNull] CalcDataRepository repository,
                                [NotNull] FileFactoryAndTracker fft
        ) : base(repository, AutomationUtili.GetOptionList(CalcOption.LocationCarpetPlot), calculationProfiler,
            "Location Carpet Plot")
        {
            _calcParameters = Repository.CalcParameters;
            _fft = fft;
        }

        [NotNull] private readonly ColourGenerator _cg = new ColourGenerator();
        protected override void PerformActualStep([NotNull] IStepParameters parameters)
        {
            HouseholdStepParameters hhp = (HouseholdStepParameters)parameters;
            if (hhp.Key.KeyType != HouseholdKeyType.Household) {
                return;
            }
            var hhkey = hhp.Key.HouseholdKey;

            TimeSpan officialSimulationDuration = _calcParameters.OfficialEndTime - _calcParameters.OfficialStartTime;
            CategoryCarpetPlotMaker ccpm = new CategoryCarpetPlotMaker(_calcParameters);
            ReadActivities(hhkey,out var locationsByPerson, out var sitesByPerson);
            int chartindex = 1;
            foreach (KeyValuePair<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>> pair in locationsByPerson) {
                string fileName = "LocationCarpetPlot" + hhp.Key.HouseholdKey.Key + "." + pair.Key.Name + ".png" ;
                 string legendFilename = "LocationCarpetPlot" + hhp.Key.HouseholdKey.Key + "." + pair.Key.Name + ".Legend.png";
                const string filedescription = "";
                 const string legendDescription = "";
                ccpm.MakeCarpet(_fft, pair.Value, officialSimulationDuration, hhp.Key.HouseholdKey,
                    Repository.CarpetPlotColumnWidth, fileName, filedescription, 1, legendFilename, legendDescription, chartindex.ToString());
                chartindex++;
            }
            if (_calcParameters.IsInTranportMode(hhp.Key.HouseholdKey)) {
                foreach (KeyValuePair<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>> pair in sitesByPerson) {
                    string fileName = "SiteCarpetPlot" + hhp.Key.HouseholdKey.Key + "." + pair.Key.Name + ".bmp";
                    string legendFilename = "SiteCarpetPlot" + hhp.Key.HouseholdKey.Key + "." + pair.Key.Name +
                                            ".Legend.bmp";
                    const string filedescription = "";
                    const string legendDescription = "";
                    ccpm.MakeCarpet(_fft, pair.Value, officialSimulationDuration, hhp.Key.HouseholdKey,
                        Repository.CarpetPlotColumnWidth, fileName, filedescription, 1, legendFilename, legendDescription,
                        chartindex.ToString());
                    chartindex++;
                }
            }
        }

        private void ReadActivities([NotNull] HouseholdKey householdKey,
                                    [NotNull] out Dictionary<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>> locationsByPerson,
                                    [NotNull] out Dictionary<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>> sitesByPerson)
        {
            var personStatuses = Repository.LoadPersonStatus(householdKey);
            List<CalcPersonDto> persons = Repository.GetPersons(householdKey);
            Dictionary<string, CalcPersonDto> personsByGuid = persons.ToDictionary(p => p.Guid, p=> p);
            locationsByPerson =new  Dictionary<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>>();
            sitesByPerson = new Dictionary<CalcPersonDto, Dictionary<int, CarpetCategoryEntry>>();
            Dictionary<string, Color> locColors = new Dictionary<string, Color>();
            Dictionary<string, Color> siteColors = new Dictionary<string, Color>();
            foreach (var entry in personStatuses) {
                var person = personsByGuid[entry.PersonGuid];
                if (!locationsByPerson.ContainsKey(person)) {
                    locationsByPerson.Add(person, new Dictionary<int, CarpetCategoryEntry>());
                    sitesByPerson.Add(person, new Dictionary<int, CarpetCategoryEntry>());
                }

                Color locColor = GetColor(entry.LocationGuid, locColors);
                CarpetCategoryEntry locEntry =
                    new CarpetCategoryEntry(entry.TimeStep, entry.LocationName, locColor, false);
                locationsByPerson[person].Add(entry.TimeStep.InternalStep, locEntry);
                if ( _calcParameters.IsInTranportMode(householdKey)) {
                    Color siteColor = GetColor(entry.SiteGuid, siteColors);
                    CarpetCategoryEntry siteEntry =
                        new CarpetCategoryEntry(entry.TimeStep, entry.SiteName, siteColor, false);
                    sitesByPerson[person].Add(entry.TimeStep.InternalStep, siteEntry);
                }
            }
        }

        private Color GetColor([CanBeNull] string key, [NotNull] Dictionary<string, Color> colors)
        {
            if (key == null) {
                return Color.FromRgb(255,255,255);
            }
            if (!colors.ContainsKey(key)) {
                string htmlc = _cg.NextColour();
                var c1 = ColorTranslator.FromHtml("#"+htmlc);
                Color c = Color.FromRgb(c1.R,c1.G,c1.B);
                colors.Add(key,c);
                return c;
            }
            return colors[key];
        }
    }
}
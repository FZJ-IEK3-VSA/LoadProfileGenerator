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

using System.Diagnostics.CodeAnalysis;

namespace Automation.ResultFiles {
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum ResultFileID {
        Actions = 0,
        Dump = 1,
        ActivationsPerHour = 2,
        ActivationFrequencies = 3,
        Totals = 4,
        DesireFiles = 5,
        DeviceProfileCSV = 6,
        DeviceProfileCSVExternal = 7,
        CSVSumProfile = 8,
        CSVSumProfileExternal = 9,
        DeviceSums = 10,
        CarpetPlots = 11,
        CarpetPlotsLegend = 12,
        SettlementIndividualSumProfile = 13,
        SettlementTotalProfile = 14,
        ThoughtsPerPerson = 15,
        CarpetPlotsEnergy = 16,
        OnlineDeviceActivationFiles = 17,
        SettlementIndividualSumProfileExternal = 18,
        SettlementTotalProfileExternal = 19,
        AffordanceEnergyUse = 20,
        WeekdayLoadProfileID = 21,
        TimeOfUse =22,
        TimeOfUseEnergy = 23,

        DurationCurveSums = 24,
        DurationCurveDevices = 25,
        CarpetPlotLabeled = 26,
        ExecutedActionsOverview = 27,
        DeviceTaggingSetFiles = 28,
        Locations = 29,
        AffordanceTimeUse = 30,
        AffordancePersonEnergyUse = 31,
        DeviceSumsPerMonth = 32,
        FiveMinuteImportFile = 33,

        CriticalThresholdViolations = 34,
        ActivityPercentages = 35,
        SettlementTotal = 36,
        Temperatures = 37,
        Daylight = 38,
        EnergyStorages = 39,
        PersonFile = 40,
        HouseholdPlanTime = 41,
        HouseholdNameFile = 42,
        DumpTime = 43,
        OnlineSumActivationFiles = 44,
        DeviceProfileForHouseholds = 45,
        SumProfileForHouseholds = 46,
        TotalsPerHousehold = 47,
        ExternalSumsForHouseholds = 48,
        DeviceProfileCSVExternalForHouseholds = 49, // external csv files
        FiveMinuteImportFileForHH = 50,
        VariableLogfile = 51,
        LocationStatistic = 52,
        ActionsPerStep = 53,
        BridgeDays = 54,
        VacationDays = 55,
        DeviceTags = 56,
        AffordanceTaggingSetFiles = 57,
        OverallSumFile = 58,
        AffordanceTagsFile = 59,
        PolysunImportFile = 60,
        PolysunImportFileHH = 61,
        AffordanceTimeUse2 = 62,
        AffordanceDefinition = 63,
        Seed = 64,
        Chart = 65,
            PDF = 66,
        JsonResultFileList= 67,
        Logfile = 68,
        HouseContents = 69,
        MissingTags = 70,
        LogfileForErrors =71,
        ResultFileXML = 72,
        TotalsJson = 73,
        AffordanceTimeUseJson = 74,
        DeviceSumsJson = 75,
        AffordanceInformation = 76,
        ActionsPerStepJson = 77,
        ActionsJson = 78,
        Transportation = 79,
        TransportationStatistics = 80,
        Sqlite = 81,
        ExternalSumsForHouseholdsJson
    }
}
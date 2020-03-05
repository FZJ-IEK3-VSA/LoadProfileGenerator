using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;

namespace Common.Enums {

    public static class CalcOptionHelper {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [NotNull]
        public static  Dictionary<CalcOption, string> CalcOptionDictionary { get; } =
            new Dictionary<CalcOption, string> {
                {CalcOption.IndividualSumProfiles, "Individual sum profile files"},
                {CalcOption.OverallSum, "Overall sum profile file"},
                {CalcOption.ActionCarpetPlot, "Action carpet plots"},
                {CalcOption.EnergyCarpetPlot, "Energy carpet plots"},
                {CalcOption.TimeOfUsePlot, "Time of use statistic"},
                {CalcOption.VariableLogFile, "Logfile with the status of the variables in each timestep"},
                {CalcOption.ActivationsPerHour, "Device Activations per hour statistic"},
                {CalcOption.DaylightTimesList, "Calculated daylight times statistic"},
                {CalcOption.ActivationFrequencies, "Frequency of device activations statistic"},
                {CalcOption.ActionsLogfile, "Detailed device activation tracking"},
                {CalcOption.DeviceProfiles, "Device Profiles"},
                {CalcOption.TotalsPerLoadtype, "Totals per load type"},
                {CalcOption.HouseholdContents, "Household contents"},
                {CalcOption.TemperatureFile, "Temperature file"},
                {CalcOption.TotalsPerDevice, "Totals per device"},
                {CalcOption.EnergyStorageFile, "Energy storage status"},
                {CalcOption.DurationCurve, "Duration Curve"},
                {CalcOption.DesiresLogfile, "Desires logfile"},
                {CalcOption.ThoughtsLogfile, "Thoughts logfile"},
                {CalcOption.PolysunImportFiles, "Files for the import in Polysun, 15 min resolution"},
                {CalcOption.CriticalViolations, "Critical threshold violations"},
                {CalcOption.SumProfileExternalEntireHouse, "External Sum Profiles for the entire house"},
                {CalcOption.SumProfileExternalIndividualHouseholds, "External Sum Profiles for the individual households"},
                {CalcOption.WeekdayProfiles, "List of averaged weekday profiles"},
                {CalcOption.AffordanceEnergyUse, "Affordance energy use statistic"},
                {CalcOption.TimeProfileFile, "Time profile file"},
                {CalcOption.LocationsFile, "Locations file"},
                {CalcOption.HouseholdPlan, "Household plan comparison"},
                {CalcOption.DeviceProfileExternalEntireHouse, "Device profile file in external time resolution for the entire house"},
                {CalcOption.DeviceProfileExternalIndividualHouseholds, "Device profile file in external time resolution for the individual households"},
                {CalcOption.MakeGraphics, "Create charts for all statistics"},
                {CalcOption.MakePDF, "Create a PDF from all the charts"},
                {CalcOption.TransportationStatistics,"Transportation Statistics"},
                {CalcOption.LogAllMessages,"Save calculation messages to database "},
                {CalcOption.LogErrorMessages,"Save calculation error messages to database"},
                {CalcOption.TransportationDeviceCarpetPlot,"Transportation device carpet plot"},
                {CalcOption.PersonStatus,"Person Status"},
                {CalcOption.LocationCarpetPlot,"Location carpet plots"},
            };
    }
}
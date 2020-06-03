using JetBrains.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automation;

namespace Common.Enums {

    public static class CalcOptionHelper {
        static CalcOptionHelper()
        {
            CalcOptionDictionary = new Dictionary<CalcOption, string>();
            CalcOptionDictionary.Add(CalcOption.HouseSumProfilesFromDetailedDats, "Individual sum profile files");
            CalcOptionDictionary.Add(CalcOption.OverallSum, "Overall sum profile file");
            CalcOptionDictionary.Add(CalcOption.ActionCarpetPlot, "Action carpet plots");
            CalcOptionDictionary.Add(CalcOption.EnergyCarpetPlot, "Energy carpet plots");
            CalcOptionDictionary.Add(CalcOption.TimeOfUsePlot, "Time of use statistic");
            CalcOptionDictionary.Add(CalcOption.VariableLogFile, "Logfile with the status of the variables in each timestep");
            CalcOptionDictionary.Add(CalcOption.ActivationsPerHour, "Device Activations per hour statistic");
            CalcOptionDictionary.Add(CalcOption.DaylightTimesList, "Calculated daylight times statistic");
            CalcOptionDictionary.Add(CalcOption.ActivationFrequencies, "Frequency of device activations statistic");
            //CalcOptionDictionary.Add(CalcOption.ActionsLogfile, "Detailed device activation tracking");
            CalcOptionDictionary.Add(CalcOption.DeviceProfilesIndividualHouseholds, "Device Profiles");
            CalcOptionDictionary.Add(CalcOption.TotalsPerLoadtype, "Totals per load type");
            CalcOptionDictionary.Add(CalcOption.HouseholdContents, "Household contents");
            CalcOptionDictionary.Add(CalcOption.TemperatureFile, "Temperature file");
            CalcOptionDictionary.Add(CalcOption.TotalsPerDevice, "Totals per device");
            CalcOptionDictionary.Add(CalcOption.EnergyStorageFile, "Energy storage status");
            CalcOptionDictionary.Add(CalcOption.DurationCurve, "Duration Curve");
            CalcOptionDictionary.Add(CalcOption.DesiresLogfile, "Desires logfile");
            CalcOptionDictionary.Add(CalcOption.ThoughtsLogfile, "Thoughts logfile");
            CalcOptionDictionary.Add(CalcOption.PolysunImportFiles, "Files for the import in Polysun, 15 min resolution");
            CalcOptionDictionary.Add(CalcOption.CriticalViolations, "Critical threshold violations");
            CalcOptionDictionary.Add(CalcOption.SumProfileExternalEntireHouse, "External Sum Profiles for the entire house");
            CalcOptionDictionary.Add(CalcOption.SumProfileExternalIndividualHouseholds, "External Sum Profiles for the individual households");
            CalcOptionDictionary.Add(CalcOption.WeekdayProfiles, "List of averaged weekday profiles");
            CalcOptionDictionary.Add(CalcOption.AffordanceEnergyUse, "Affordance energy use statistic");
            CalcOptionDictionary.Add(CalcOption.TimeProfileFile, "Time profile file");
            CalcOptionDictionary.Add(CalcOption.LocationsFile, "Locations file");
            CalcOptionDictionary.Add(CalcOption.HouseholdPlan, "Household plan comparison");
            CalcOptionDictionary.Add(CalcOption.DeviceProfileExternalEntireHouse, "Device profile file in external time resolution for the entire house");
            CalcOptionDictionary.Add(CalcOption.DeviceProfileExternalIndividualHouseholds, "Device profile file in external time resolution for the individual households");
            CalcOptionDictionary.Add(CalcOption.MakeGraphics, "Create charts for all statistics");
            CalcOptionDictionary.Add(CalcOption.MakePDF, "Create a PDF from all the charts");
            CalcOptionDictionary.Add(CalcOption.TransportationStatistics, "Transportation Statistics");
            CalcOptionDictionary.Add(CalcOption.LogAllMessages, "Save calculation messages to database ");
            CalcOptionDictionary.Add(CalcOption.LogErrorMessages, "Save calculation error messages to database");
            CalcOptionDictionary.Add(CalcOption.TransportationDeviceCarpetPlot, "Transportation device carpet plot");
            CalcOptionDictionary.Add(CalcOption.PersonStatus, "Person Status");
            CalcOptionDictionary.Add(CalcOption.LocationCarpetPlot, "Location carpet plots");
            CalcOptionDictionary.Add(CalcOption.JsonHouseSumFiles, "Json overall sum Profiles");
        }

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [NotNull]
        public static  Dictionary<CalcOption, string> CalcOptionDictionary { get; }
    }
}
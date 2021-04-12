using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automation;

namespace Common {
    public static class OutputFileDefaultHelper {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        [JetBrains.Annotations.NotNull]
        public static Dictionary<OutputFileDefault, string> OutputFileDefaultDictionary { get; } =
            new Dictionary<OutputFileDefault, string> {
                [OutputFileDefault.All] = "All files",
                [OutputFileDefault.OnlyOverallSum] = "Only the overall sum profile file",
                [OutputFileDefault.OnlySums] = "Only the sum files, both for the households and the house",
                [OutputFileDefault.OnlyDeviceProfiles] = "Only the device profiles",
                [OutputFileDefault.Reasonable] = "A reasonable subset",
                [OutputFileDefault.NoFiles] = "No files at all",
                [OutputFileDefault.ForSettlementCalculations] = "For Settlement calculations"
            };

        //[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        //public static readonly Dictionary<OutputFileDefault, string> OutputFileDefaultDictionaryFull =
        //    new Dictionary<OutputFileDefault, string> {
        //        {OutputFileDefault.All, "All files"},
        //        {OutputFileDefault.OnlyOverallSum, "Only the overall sum profile file"},
        //        {OutputFileDefault.OnlySums, "Only the sum files, both for the households and the house"},
        //        {OutputFileDefault.OnlyDeviceProfiles, "Only the device profiles"},
        //        {OutputFileDefault.Reasonable, "A reasonable subset"},
        //        {OutputFileDefault.ReasonableWithCharts, "A reasonable subset with charts"},
        //        {OutputFileDefault.ReasonableWithChartsAndPDF, "A reasonable subset with the PDF report"},
        //        {OutputFileDefault.None, "No files at all"}
        //    };

        [JetBrains.Annotations.NotNull]
        public static List<CalcOption> GetOptionsForDefault(OutputFileDefault selectedOptionDefault) {
            var l = new List<CalcOption>();
            switch (selectedOptionDefault) {
                case OutputFileDefault.All:
                    return Enum.GetValues(typeof(CalcOption)).Cast<CalcOption>().ToList();
                case OutputFileDefault.NoFiles:
                    return l;
                case OutputFileDefault.OnlyOverallSum:
                    l.Add(CalcOption.OverallSum);
                    break;
                case OutputFileDefault.OnlySums:
                    l.Add(CalcOption.HouseSumProfilesFromDetailedDats);
                    break;
                case OutputFileDefault.ReasonableWithCharts:
                    l.AddRange(GetOptionsForDefault(OutputFileDefault.Reasonable));
                    l.Add(CalcOption.MakeGraphics);
                    break;
                case OutputFileDefault.Reasonable:
                    l.Add(CalcOption.ActionCarpetPlot);
                    l.Add(CalcOption.HouseSumProfilesFromDetailedDats);
                    //l.Add(CalcOption.EnergyCarpetPlot);
                    l.Add(CalcOption.TimeOfUsePlot);
                    l.Add(CalcOption.VariableLogFile);
                    l.Add(CalcOption.ActivationsPerHour);
                    l.Add(CalcOption.ActivationFrequencies);
                    //l.Add(CalcOption.ActionsLogfile);
                    l.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    l.Add(CalcOption.TotalsPerLoadtype);
                    l.Add(CalcOption.HouseholdContents);
                    l.Add(CalcOption.TotalsPerDevice);
                    l.Add(CalcOption.EnergyStorageFile);
                    l.Add(CalcOption.DurationCurve);
                    l.Add(CalcOption.SumProfileExternalEntireHouse);
                    l.Add(CalcOption.SumProfileExternalIndividualHouseholds);
                    l.Add(CalcOption.AffordanceEnergyUse);
                    l.Add(CalcOption.TimeProfileFile);
                    l.Add(CalcOption.LocationsFile);
                    l.Add(CalcOption.HouseholdPlan);
                    l.Add(CalcOption.DeviceProfileExternalEntireHouse);
                    l.Add(CalcOption.DeviceProfileExternalIndividualHouseholds);
                    l.Add(CalcOption.WeekdayProfiles);
                    l.Add(CalcOption.LogErrorMessages);
                    l.Add(CalcOption.LogAllMessages);
                    l.Add(CalcOption.TransportationDeviceCarpetPlot);
                    break;
                case OutputFileDefault.OnlyDeviceProfiles:
                    l.Add(CalcOption.HouseSumProfilesFromDetailedDats);
                    l.Add(CalcOption.SumProfileExternalEntireHouse);
                    l.Add(CalcOption.SumProfileExternalIndividualHouseholds);
                    l.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    l.Add(CalcOption.DeviceProfileExternalEntireHouse);
                    l.Add(CalcOption.DeviceProfileExternalIndividualHouseholds);
                    l.Add(CalcOption.LogErrorMessages);
                    l.Add(CalcOption.LogAllMessages);
                    break;
                case OutputFileDefault.ReasonableWithChartsAndPDF:
                    l.AddRange(GetOptionsForDefault(OutputFileDefault.Reasonable));
                    l.Add(CalcOption.MakePDF);
                    break;
                case OutputFileDefault.ForSettlementCalculations:
                    l.Add(CalcOption.HouseSumProfilesFromDetailedDats);
                    l.Add(CalcOption.TotalsPerLoadtype);
                    l.Add(CalcOption.HouseholdContents);
                    l.Add(CalcOption.TotalsPerDevice);
                    l.Add(CalcOption.ActivationFrequencies);
                    l.Add(CalcOption.DeviceProfilesIndividualHouseholds);
                    l.Add(CalcOption.SumProfileExternalEntireHouse);
                    l.Add(CalcOption.SumProfileExternalIndividualHouseholds);
                    l.Add(CalcOption.LogErrorMessages);
                    l.Add(CalcOption.LogAllMessages);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectedOptionDefault), selectedOptionDefault, null);
            }
            return l;
        }
    }
}
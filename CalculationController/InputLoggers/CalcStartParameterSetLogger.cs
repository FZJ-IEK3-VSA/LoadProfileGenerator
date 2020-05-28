using Automation;
using Automation.ResultFiles;
using CalculationController.Queue;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CalculationController.InputLoggers
{
    using System.Globalization;

    public class CalcStartParameterSetLogger : DataSaverBase
    {
        public CalcStartParameterSetLogger( [NotNull] SqlResultLoggingService srls): base(typeof(CalcStartParameterSet),
            new ResultTableDefinition("CalcStartParameterSet",ResultTableID.CalcStartParameters, "All the calculation start parameters", CalcOption.BasicOverview), srls)
        {
        }

        public override void Run(HouseholdKey key,object o)
        {
            CalcStartParameterSet csps = (CalcStartParameterSet)o;
            SaveableEntry se = new SaveableEntry(key, ResultTableDefinition);
            se.AddField("Name",SqliteDataType.Text);
            se.AddField("Value", SqliteDataType.Text);
            se.AddRow(RowBuilder.Start("Name", "CsvCharacter").Add("Value", csps.CsvCharacter).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "Temperature Profile").Add("Value", csps.TemperatureProfile.Name).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "AffordanceRepetitionCount").Add("Value", csps.AffordanceRepetitionCount.ToString(CultureInfo.InvariantCulture)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "CalcOptions").Add("Value", JsonConvert.SerializeObject(csps.CalcOptions,Formatting.Indented)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "CalcTarget").Add("Value", csps.CalcTarget.Name).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "DeleteDatFiles").Add("Value", csps.DeleteDatFiles.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "DeviceSelection").Add("Value", csps.DeviceSelection?.Name??"None").ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "EnergyIntensity").Add("Value", csps.EnergyIntensity.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "ExternalTimeResolution").Add("Value", csps.ExternalTimeResolution.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "GeographicLocation").Add("Value", csps.GeographicLocation.Name).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "InternalTimeResolution").Add("Value", csps.InternalTimeResolution.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "LPGVersion").Add("Value", csps.LPGVersion).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "LoadTypePriority").Add("Value", csps.LoadTypePriority.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "OfficialSimulationStartTime").Add("Value", csps.OfficialSimulationStartTime.ToString(CultureInfo.InvariantCulture)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "OfficialSimulationEndTime").Add("Value", csps.OfficialSimulationEndTime.ToString(CultureInfo.InvariantCulture)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "SelectedRandomSeed").Add("Value", csps.SelectedRandomSeed.ToString(CultureInfo.InvariantCulture)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "SettlingDays").Add("Value", csps.SettlingDays.ToString(CultureInfo.InvariantCulture)).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "ShowSettlingPeriod").Add("Value", csps.ShowSettlingPeriod.ToString()).ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "TransportationDeviceSet").Add("Value", csps.TransportationDeviceSet?.Name??"None").ToDictionary());
            se.AddRow(RowBuilder.Start("Name", "TravelRouteSet").Add("Value", csps.TravelRouteSet?.Name ?? "None").ToDictionary());
            if(Srls == null) {
                throw new LPGException("Data Logger was null.");
            }

            Srls.SaveResultEntry(se);
        }
    }
}

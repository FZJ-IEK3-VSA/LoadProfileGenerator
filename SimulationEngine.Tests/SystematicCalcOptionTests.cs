using Automation;
using Xunit;
using Common.Tests;
using Xunit.Abstractions;
using JetBrains.Annotations;
#pragma warning disable 8602
namespace SimulationEngine.Tests {
public class SystematicCalcOptionTests :UnitTestBaseClass {

[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsIndividualSumProfiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.IndividualSumProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsOverallDats(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.OverallDats;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsOverallSum(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.OverallSum;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDetailedDatFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DetailedDatFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActionCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsEnergyCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.EnergyCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTimeOfUsePlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TimeOfUsePlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsVariableLogFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.VariableLogFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActivationsPerHour(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActivationsPerHour;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDaylightTimesList(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DaylightTimesList;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActivationFrequencies(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActivationFrequencies;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTotalsPerLoadtype(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TotalsPerLoadtype;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsHouseholdContents(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdContents;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTemperatureFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TemperatureFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTotalsPerDevice(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TotalsPerDevice;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsEnergyStorageFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.EnergyStorageFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDurationCurve(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DurationCurve;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDesiresLogfile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DesiresLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsThoughtsLogfile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ThoughtsLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsPolysunImportFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.PolysunImportFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsCriticalViolations(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.CriticalViolations;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalEntireHouse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsWeekdayProfiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.WeekdayProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsAffordanceEnergyUse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.AffordanceEnergyUse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTimeProfileFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TimeProfileFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLocationsFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationsFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsHouseholdPlan(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdPlan;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfileExternalEntireHouse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfileExternalIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsMakeGraphics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.MakeGraphics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsMakePDF(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.MakePDF;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLocationCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsPersonStatus(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.PersonStatus;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTransportationDeviceCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TransportationDeviceCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLogErrorMessages(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LogErrorMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLogAllMessages(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LogAllMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTransportationStatistics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TransportationStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActionsEachTimestep(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionsEachTimestep;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsCalculationFlameChart(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.CalculationFlameChart;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalIndividualHouseholdsAsJson(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholdsAsJson;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsJsonSumFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.JsonSumFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsBodilyActivityStatistics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.BodilyActivityStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsBasicOverview(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.BasicOverview;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceActivations(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceActivations;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLocationsEntries(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationsEntries;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActionEntries(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionEntries;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsAffordanceTaggingSets(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.AffordanceTaggingSets;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}

public SystematicCalcOptionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
}}

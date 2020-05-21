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
      const CalcOption co = CalcOption.IndividualSumProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsOverallDats(){
      const CalcOption co = CalcOption.OverallDats;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsOverallSum(){
      const CalcOption co = CalcOption.OverallSum;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDetailedDatFiles(){
      const CalcOption co = CalcOption.DetailedDatFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActionCarpetPlot(){
      const CalcOption co = CalcOption.ActionCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsEnergyCarpetPlot(){
      const CalcOption co = CalcOption.EnergyCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTimeOfUsePlot(){
      const CalcOption co = CalcOption.TimeOfUsePlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsVariableLogFile(){
      const CalcOption co = CalcOption.VariableLogFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActivationsPerHour(){
      const CalcOption co = CalcOption.ActivationsPerHour;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDaylightTimesList(){
      const CalcOption co = CalcOption.DaylightTimesList;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActivationFrequencies(){
      const CalcOption co = CalcOption.ActivationFrequencies;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfiles(){
      const CalcOption co = CalcOption.DeviceProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTotalsPerLoadtype(){
      const CalcOption co = CalcOption.TotalsPerLoadtype;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsHouseholdContents(){
      const CalcOption co = CalcOption.HouseholdContents;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTemperatureFile(){
      const CalcOption co = CalcOption.TemperatureFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTotalsPerDevice(){
      const CalcOption co = CalcOption.TotalsPerDevice;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsEnergyStorageFile(){
      const CalcOption co = CalcOption.EnergyStorageFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDurationCurve(){
      const CalcOption co = CalcOption.DurationCurve;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDesiresLogfile(){
      const CalcOption co = CalcOption.DesiresLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsThoughtsLogfile(){
      const CalcOption co = CalcOption.ThoughtsLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsPolysunImportFiles(){
      const CalcOption co = CalcOption.PolysunImportFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsCriticalViolations(){
      const CalcOption co = CalcOption.CriticalViolations;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalEntireHouse(){
      const CalcOption co = CalcOption.SumProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalIndividualHouseholds(){
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsWeekdayProfiles(){
      const CalcOption co = CalcOption.WeekdayProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsAffordanceEnergyUse(){
      const CalcOption co = CalcOption.AffordanceEnergyUse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTimeProfileFile(){
      const CalcOption co = CalcOption.TimeProfileFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLocationsFile(){
      const CalcOption co = CalcOption.LocationsFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsHouseholdPlan(){
      const CalcOption co = CalcOption.HouseholdPlan;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfileExternalEntireHouse(){
      const CalcOption co = CalcOption.DeviceProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsDeviceProfileExternalIndividualHouseholds(){
      const CalcOption co = CalcOption.DeviceProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsMakeGraphics(){
      const CalcOption co = CalcOption.MakeGraphics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsMakePDF(){
      const CalcOption co = CalcOption.MakePDF;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLocationCarpetPlot(){
      const CalcOption co = CalcOption.LocationCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsPersonStatus(){
      const CalcOption co = CalcOption.PersonStatus;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTransportationDeviceCarpetPlot(){
      const CalcOption co = CalcOption.TransportationDeviceCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLogErrorMessages(){
      const CalcOption co = CalcOption.LogErrorMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsLogAllMessages(){
      const CalcOption co = CalcOption.LogAllMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsTransportationStatistics(){
      const CalcOption co = CalcOption.TransportationStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsActionsEachTimestep(){
      const CalcOption co = CalcOption.ActionsEachTimestep;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsCalculationFlameChart(){
      const CalcOption co = CalcOption.CalculationFlameChart;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsSumProfileExternalIndividualHouseholdsAsJson(){
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholdsAsJson;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsJsonSumFiles(){
      const CalcOption co = CalcOption.JsonSumFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsBodilyActivityStatistics(){
      const CalcOption co = CalcOption.BodilyActivityStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest6)]
public void TestHouseJobsBasicOverview(){
      const CalcOption co = CalcOption.BasicOverview;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForOutputFileTesting(sim);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}

public SystematicCalcOptionTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
}}

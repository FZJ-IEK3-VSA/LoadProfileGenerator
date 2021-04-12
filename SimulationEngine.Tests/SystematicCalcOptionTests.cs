using Automation;
using Xunit;
using Common.Tests;
using Xunit.Abstractions;
using JetBrains.Annotations;
#pragma warning disable 8602
namespace SimulationEngine.Tests {
public class SystematicCalcOptionTests :UnitTestBaseClass {

[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsHouseSumProfilesFromDetailedDats(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseSumProfilesFromDetailedDats;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsOverallDats(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.OverallDats;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsOverallSum(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.OverallSum;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDetailedDatFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DetailedDatFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsActionCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTimeOfUsePlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TimeOfUsePlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsVariableLogFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.VariableLogFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsActivationsPerHour(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActivationsPerHour;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDaylightTimesList(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DaylightTimesList;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsActivationFrequencies(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActivationFrequencies;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceProfilesIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfilesIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTotalsPerLoadtype(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TotalsPerLoadtype;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsHouseholdContents(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdContents;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTemperatureFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TemperatureFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTotalsPerDevice(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TotalsPerDevice;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsEnergyStorageFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.EnergyStorageFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDurationCurve(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DurationCurve;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDesiresLogfile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DesiresLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsThoughtsLogfile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ThoughtsLogfile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsPolysunImportFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.PolysunImportFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsCriticalViolations(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.CriticalViolations;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsSumProfileExternalEntireHouse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsSumProfileExternalIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsWeekdayProfiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.WeekdayProfiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsAffordanceEnergyUse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.AffordanceEnergyUse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTimeProfileFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TimeProfileFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsLocationsFile(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationsFile;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsHouseholdPlan(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdPlan;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceProfileExternalEntireHouse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfileExternalEntireHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceProfileExternalIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfileExternalIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsMakeGraphics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.MakeGraphics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsMakePDF(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.MakePDF;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsLocationCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsPersonStatus(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.PersonStatus;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTransportationDeviceCarpetPlot(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TransportationDeviceCarpetPlot;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsLogErrorMessages(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LogErrorMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsLogAllMessages(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LogAllMessages;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTransportationStatistics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TransportationStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsActionsEachTimestep(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionsEachTimestep;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsCalculationFlameChart(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.CalculationFlameChart;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsSumProfileExternalIndividualHouseholdsAsJson(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholdsAsJson;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsJsonHouseSumFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.JsonHouseSumFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsBodilyActivityStatistics(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.BodilyActivityStatistics;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsBasicOverview(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.BasicOverview;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceActivations(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceActivations;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsLocationsEntries(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.LocationsEntries;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsActionEntries(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.ActionEntries;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsAffordanceTaggingSets(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.AffordanceTaggingSets;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceProfilesHouse(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceProfilesHouse;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsHouseholdSumProfilesFromDetailedDats(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdSumProfilesFromDetailedDats;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsJsonHouseholdSumFiles(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.JsonHouseholdSumFiles;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsJsonDeviceProfilesIndividualHouseholds(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.JsonDeviceProfilesIndividualHouseholds;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsTansportationDeviceJsons(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.TansportationDeviceJsons;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsDeviceTaggingSets(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.DeviceTaggingSets;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsAffordanceDefinitions(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.AffordanceDefinitions;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsJsonHouseholdSumFilesNoFlex(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.JsonHouseholdSumFilesNoFlex;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsHouseholdSumProfilesCsvNoFlex(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.HouseholdSumProfilesCsvNoFlex;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
public void TestHouseJobsFlexibilityEvents(){
      const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
      const CalcOption co = CalcOption.FlexibilityEvents;
      HouseJobTestHelper.RunSingleHouse((sim) => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid,TestDuration.TwelveMonths);
      hj.CalcSpec.CalcOptions.Add(co); return hj;
      }, (x) => HouseJobTestHelper.CheckForResultfile(x, co));
}

public SystematicCalcOptionTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
}}

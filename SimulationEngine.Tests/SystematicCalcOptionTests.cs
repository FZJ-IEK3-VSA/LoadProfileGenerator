using Automation;
using Xunit;
using Common.Tests;
using Xunit.Abstractions;
using Common;
#pragma warning disable 8602
namespace SimulationEngine.Tests
{
    public class SystematicCalcOptionTests : UnitTestBaseClass
    {
        private static HouseCreationAndCalculationJob MakeHouseJob(Database.Simulator sim, string hhguid, CalcOption co)
        {
            var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTestingWithTransport(sim, hhguid, TestDuration.TwelveMonths);
            hj.CalcSpec.CalcOptions.Add(co);
            return hj;
        }

        private static void RunHousejobWithResultFileCheck(string hhguid, CalcOption co)
        {
            string testname = Utili.GetCallingMethodAndClass();
            HouseJobTestHelper.RunSingleHouse(sim => MakeHouseJob(sim, hhguid, co), (x) => HouseJobTestHelper.CheckForResultfile(x, co), testname: testname);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsHouseSumProfilesFromDetailedDats()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseSumProfilesFromDetailedDats;
            RunHousejobWithResultFileCheck(hhguid, co);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsOverallDats()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.OverallDats;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsOverallSum()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.OverallSum;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDetailedDatFiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DetailedDatFiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsActionCarpetPlot()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ActionCarpetPlot;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTimeOfUsePlot()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TimeOfUsePlot;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsVariableLogFile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.VariableLogFile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsActivationsPerHour()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ActivationsPerHour;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDaylightTimesList()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DaylightTimesList;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsActivationFrequencies()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ActivationFrequencies;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceProfilesIndividualHouseholds()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceProfilesIndividualHouseholds;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTotalsPerLoadtype()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TotalsPerLoadtype;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsHouseholdContents()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseholdContents;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTemperatureFile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TemperatureFile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTotalsPerDevice()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TotalsPerDevice;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsEnergyStorageFile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.EnergyStorageFile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDurationCurve()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DurationCurve;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDesiresLogfile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DesiresLogfile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsThoughtsLogfile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ThoughtsLogfile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsPolysunImportFiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.PolysunImportFiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsCriticalViolations()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.CriticalViolations;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsSumProfileExternalEntireHouse()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.SumProfileExternalEntireHouse;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsSumProfileExternalIndividualHouseholds()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholds;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsWeekdayProfiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.WeekdayProfiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsAffordanceEnergyUse()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.AffordanceEnergyUse;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTimeProfileFile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TimeProfileFile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsLocationsFile()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.LocationsFile;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsHouseholdPlan()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseholdPlan;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceProfileExternalEntireHouse()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceProfileExternalEntireHouse;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceProfileExternalIndividualHouseholds()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceProfileExternalIndividualHouseholds;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsMakeGraphics()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.MakeGraphics;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsMakePDF()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.MakePDF;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsLocationCarpetPlot()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.LocationCarpetPlot;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsPersonStatus()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.PersonStatus;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTransportationDeviceCarpetPlot()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TransportationDeviceCarpetPlot;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsLogErrorMessages()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.LogErrorMessages;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsLogAllMessages()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.LogAllMessages;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTransportationStatistics()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TransportationStatistics;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsActionsEachTimestep()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ActionsEachTimestep;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsCalculationFlameChart()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.CalculationFlameChart;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsSumProfileExternalIndividualHouseholdsAsJson()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.SumProfileExternalIndividualHouseholdsAsJson;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsJsonHouseSumFiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonHouseSumFiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsBodilyActivityStatistics()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.BodilyActivityStatistics;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsBasicOverview()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.BasicOverview;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceActivations()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceActivations;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsLocationsEntries()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.LocationsEntries;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsActionEntries()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.ActionEntries;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsAffordanceTaggingSets()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.AffordanceTaggingSets;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceProfilesHouse()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceProfilesHouse;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsHouseholdSumProfilesFromDetailedDats()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseholdSumProfilesFromDetailedDats;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsJsonHouseholdSumFiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonHouseholdSumFiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsJsonDeviceProfilesIndividualHouseholds()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonDeviceProfilesIndividualHouseholds;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsTansportationDeviceJsons()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.TansportationDeviceJsons;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeviceTaggingSets()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeviceTaggingSets;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsAffordanceDefinitions()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.AffordanceDefinitions;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsJsonHouseholdSumFilesNoFlex()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.JsonHouseholdSumFilesNoFlex;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsHouseholdSumProfilesCsvNoFlex()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.HouseholdSumProfilesCsvNoFlex;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsFlexibilityEvents()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.FlexibilityEvents;
            RunHousejobWithResultFileCheck(hhguid, co);
        }


        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.CalcOptionTests)]
        public void TestHouseJobsDeleteDatFiles()
        {
            const string hhguid = "516a33ab-79e1-4221-853b-967fc11cc85a";
            const CalcOption co = CalcOption.DeleteDatFiles;
            RunHousejobWithResultFileCheck(hhguid, co);
        }

        public SystematicCalcOptionTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
}

using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests.SQLResultLogging
{
    public class SqlResultLoggingTest : UnitTestBaseClass
    {
        /*
        private class TestDataClass  {
            public TestDataClass([JetBrains.Annotations.NotNull] string name, [JetBrains.Annotations.NotNull] HouseholdKey key)
            {
                Name = name;
                HouseholdKey = key;
            }

            public int ID { get; set; }
            public HouseholdKey HouseholdKey { get; }
            [JetBrains.Annotations.NotNull]
            public string Name { [UsedImplicitly] get; }
        }*/

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void CreateTableFromFieldlistTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.ClearDirectory();
                var srl = new SqlResultLoggingService(wd.WorkingDirectory);
                SqlResultLoggingService.FieldDefinition fd = new SqlResultLoggingService.FieldDefinition("name", "text");
                List<SqlResultLoggingService.FieldDefinition> fields = new List<SqlResultLoggingService.FieldDefinition>
            {
                fd
            };
                srl.MakeTableForListOfFields(fields, new HouseholdKey("hh0"), "tbl1");
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SaveDictionaryTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.ClearDirectory();
                var srl = new SqlResultLoggingService(wd.WorkingDirectory);
                SqlResultLoggingService.FieldDefinition fd = new SqlResultLoggingService.FieldDefinition("name", "text");
                List<SqlResultLoggingService.FieldDefinition> fields = new List<SqlResultLoggingService.FieldDefinition>
            {
                fd
            };
                var hhkey = new HouseholdKey("hh0");
                srl.MakeTableForListOfFields(fields, hhkey, "tbl1");
                Dictionary<string, object> values = new Dictionary<string, object>
            {
                { "name", "blub" }
            };
                srl.SaveDictionaryToDatabaseNewConnection(values, "tbl1", hhkey);
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void DoubleSaveDictionaryTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.ClearDirectory();
                var srl = new SqlResultLoggingService(wd.WorkingDirectory);
                SqlResultLoggingService.FieldDefinition fd = new SqlResultLoggingService.FieldDefinition("name", "text");
                List<SqlResultLoggingService.FieldDefinition> fields = new List<SqlResultLoggingService.FieldDefinition>
            {
                fd
            };
                var hhkey = new HouseholdKey("hh0");
                srl.MakeTableForListOfFields(fields, hhkey, "tbl1");
                Dictionary<string, object> values = new Dictionary<string, object>
            {
                { "name", "blub" }
            };
                srl.SaveDictionaryToDatabaseNewConnection(values, "tbl1", hhkey);
                wd.CleanUp();
            }
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void SaveDictionaryCalcParametersTest()
        {
            using (WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass()))
            {
                wd.ClearDirectory();
                var srl = new SqlResultLoggingService(wd.WorkingDirectory);
                var hhkey = new HouseholdKey("hh0");
                ResultTableDefinition rtd = new ResultTableDefinition("tbl1", ResultTableID.AffordanceDefinitions, "tabledesc", CalcOption.BasicOverview);
                SaveableEntry se = new SaveableEntry(hhkey, rtd);
                se.AddField("Name", SqliteDataType.Text);
                se.AddField("Json", SqliteDataType.Text);
                se.AddRow(RowBuilder.Start("Name", "first").Add("Json", "[]").ToDictionary());
                srl.SaveResultEntry(se);
                wd.CleanUp();
            }
        }

        public SqlResultLoggingTest([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

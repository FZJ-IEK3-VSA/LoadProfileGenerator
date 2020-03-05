using System.Collections.Generic;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using NUnit.Framework;

namespace Common.Tests.SQLResultLogging
{
    internal class SqlResultLoggingTest
    {
        /*
        private class TestDataClass  {
            public TestDataClass([NotNull] string name, [NotNull] HouseholdKey key)
            {
                Name = name;
                HouseholdKey = key;
            }

            public int ID { get; set; }
            public HouseholdKey HouseholdKey { get; }
            [NotNull]
            public string Name { [UsedImplicitly] get; }
        }*/

        [Test]
        [Category("BasicTest")]
        public void CreateTableFromFieldlistTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.ClearDirectory();
            var srl = new SqlResultLoggingService(wd.WorkingDirectory);
            SqlResultLoggingService.FieldDefinition fd = new SqlResultLoggingService.FieldDefinition("name","text");
            List<SqlResultLoggingService.FieldDefinition> fields = new List<SqlResultLoggingService.FieldDefinition>
            {
                fd
            };
            srl.MakeTableForListOfFields(fields,new HouseholdKey("hh0"),"tbl1");
            wd.CleanUp();
        }

        [Test]
        [Category("BasicTest")]
        public void SaveDictionaryTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
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
            srl.SaveDictionaryToDatabaseNewConnection(values,"tbl1",hhkey);
            wd.CleanUp();
        }

        [Test]
        [Category("BasicTest")]
        public void DoubleSaveDictionaryTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
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

        [Test]
        [Category("BasicTest")]
        public void SaveDictionaryCalcParametersTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
            wd.ClearDirectory();
            var srl = new SqlResultLoggingService(wd.WorkingDirectory);
            var hhkey = new HouseholdKey("hh0");
            ResultTableDefinition rtd = new ResultTableDefinition("tbl1",ResultTableID.AffordanceDefinitions,"tabledesc");
            SaveableEntry se = new SaveableEntry(hhkey,rtd);
            se.AddField("Name",SqliteDataType.Text);
            se.AddField("Json", SqliteDataType.Text);
            se.AddRow(RowBuilder.Start("Name","first").Add("Json","[]").ToDictionary());
            srl.SaveResultEntry(se);
            wd.CleanUp();
        }
    }
}

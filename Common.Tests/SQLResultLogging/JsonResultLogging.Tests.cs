using System.Collections.Generic;
using System.IO;
using System.Linq;
using Automation;
using Automation.ResultFiles;
using Common.SQLResultLogging;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests.SQLResultLogging
{
    public class JsonResultLoggingTest(ITestOutputHelper testOutputHelper) : UnitTestBaseClass(testOutputHelper)
    {
        /// <summary>
        /// Helper function that creates table rows for testing result logging services.
        /// </summary>
        /// <param name="name">name of the item</param>
        /// <returns>dummy table row as dictionary</returns>
        private static Dictionary<string, object> MakeRow(string name) => RowBuilder.Start("Name", name).Add("Json", "[]").ToDictionary();

        /// <summary>
        /// Tests the deletion of entries from a JSON result logging service. It verifies that entries can be deleted correctly and that the resulting JSON file remains valid after deletions.
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void DeleteEntriesTest()
        {
            using WorkingDir wd = new(Utili.GetCurrentMethodAndClass());
            wd.ClearDirectory();
            var rls = new JsonResultLoggingService(wd.WorkingDirectory);
            var hhkey = new HouseholdKey("hh0");
            const string tableName = "tbl1";
            var rows = new List<Dictionary<string, object>> { MakeRow("first"), MakeRow("second"), MakeRow("third") };
            rls.SaveDictionaryToDatabaseNewConnection(rows, tableName, hhkey);

            // delete the second entry
            rls.DeleteEntries([MakeRow("second")], tableName, hhkey);

            // the file must still exist and contain exactly the two remaining entries
            string filepath = Path.Combine(wd.WorkingDirectory, "Results." + hhkey, tableName + ".json");
            File.Exists(filepath).Should().BeTrue();
            var remaining = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(filepath));
            remaining.Should().NotBeNull();
            remaining.Select(r => r["Name"]).Should().Equal("first", "third");

            // deleting all remaining entries must leave a valid empty array
            rls.DeleteEntries([MakeRow("first"), MakeRow("third")], tableName, hhkey);
            JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(filepath)).Should().BeEmpty();

            // adding to the emptied file must still produce valid JSON
            rls.SaveDictionaryToDatabaseNewConnection(MakeRow("fourth"), tableName, hhkey);
            remaining = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(File.ReadAllText(filepath));
            remaining.Should().NotBeNull();
            remaining.Select(r => r["Name"]).Should().Equal("fourth");
            wd.CleanUp();
        }
    }
}

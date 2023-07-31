using Automation;
using Common;
using Database.Tables;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Common.Tests;
using Xunit.Abstractions;

namespace Database.Tests
{
    /// <summary>
    /// A class for tests that check the consistency and validity of the database.
    /// </summary>
    public class ConsistencyTests : UnitTestBaseClass
    {
        public ConsistencyTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <summary>
        /// This test checks whether all GUIDs of database objects are unique. This shall detect errors
        /// </summary>
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void UniqueGUIDsTest()
        {
            using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                Dictionary<StrGuid, List<Tuple<string, Type>>> items_by_guid = new Dictionary<StrGuid, List<Tuple<string, Type>>>();
                var sim = new Simulator(db.ConnectionString);

                // collect and sort all database objects by GUID
                foreach (var category in sim.Categories)
                {
                    // most category objects are of type CategoryDBBase<T>, with different T
                    // Cast to dynamic type to be able to obtain all contained DBBase objects.
                    dynamic cat = category;
                    List<DBBase> items;
                    try
                    {
                        // defined for CategoryDBBase objects
                        items = cat.CollectAllDBBaseItems();
                    }
                    catch (LPGNotImplementedException)
                    {
                        // another type of object collection, not relevant for this test
                        continue;
                    }
                    // collect name and type of each database object, by GUID
                    foreach (DBBase item in items)
                    {
                        var entry = new Tuple<string, Type>(item.Name, item.GetType());
                        if (!items_by_guid.ContainsKey(item.Guid)) {
                            // first item with this GUID, create a new list with only this item
                            items_by_guid[item.Guid] = new List<Tuple<string, Type>> { entry };
                        } else
                        {
                            // This case means that a GUID was not unique. Instead of failing the test now, all
                            // objects are checked first to provide a summary of all faulty objects in the end.
                            items_by_guid[item.Guid].Add(entry);
                        }
                    }
                }
                // filter all GUIDs which occurred more than once
                var nonunique_guids = items_by_guid.Where(pair => pair.Value.Count > 1);
                if (nonunique_guids.Any())
                {
                    string output = "";
                    // generate a summary of non-unique GUIDs and the objects that use them
                    foreach (var entry in nonunique_guids)
                    {
                        output += entry.Key + ": ";
                        output += string.Join(", ", entry.Value.Select(tpl => tpl.Item1 + " (" + tpl.Item2.Name + ")")) + "\n";
                    }
                    Assert.Fail("Found non-unique GUIDs:\n" + output);
                }
                
                db.Cleanup();
            }
        }
    }
}

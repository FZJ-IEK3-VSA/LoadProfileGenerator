using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using JetBrains.Annotations;
using NUnit.Framework;
using Xunit;
using Xunit.Abstractions;
using Assert = NUnit.Framework.Assert;

namespace Database.Tests.Tables.BasicElements
{
    [TestFixture]
    public class VariableTests : UnitTestBaseClass
    {
        [Fact]
        [Category(UnitTestCategories.BasicTest)]
        public void VariableTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Variable.TableName);
                Variable t = new Variable("blub", "desc", "unit", db.ConnectionString, System.Guid.NewGuid().ToStrGuid());
                t.SaveToDB();
                ObservableCollection<Variable> allVariables = new ObservableCollection<Variable>();
                Variable.LoadFromDatabase(allVariables, db.ConnectionString, false);
                Assert.AreEqual(1, allVariables.Count);
                allVariables[0].DeleteFromDB();
                allVariables.Clear();
                Variable.LoadFromDatabase(allVariables, db.ConnectionString, false);
                Assert.AreEqual(0, allVariables.Count);
                db.Cleanup();
            }
        }

        public VariableTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
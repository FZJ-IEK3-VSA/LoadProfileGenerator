using System.Collections.ObjectModel;
using Automation;
using Common;
using Common.Tests;
using Database.Tables.BasicElements;
using FluentAssertions;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;


namespace Database.Tests.Tables.BasicElements
{

    public class VariableTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void VariableTest()
        {
            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
            {
                db.ClearTable(Variable.TableName);
                Variable t = new Variable("blub", "desc", "unit", db.ConnectionString, System.Guid.NewGuid().ToStrGuid());
                t.SaveToDB();
                ObservableCollection<Variable> allVariables = new ObservableCollection<Variable>();
                Variable.LoadFromDatabase(allVariables, db.ConnectionString, false);
                allVariables.Count.Should().Be(1);
                allVariables[0].DeleteFromDB();
                allVariables.Clear();
                Variable.LoadFromDatabase(allVariables, db.ConnectionString, false);
                allVariables.Count.Should().Be(0);
                db.Cleanup();
            }
        }

        public VariableTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
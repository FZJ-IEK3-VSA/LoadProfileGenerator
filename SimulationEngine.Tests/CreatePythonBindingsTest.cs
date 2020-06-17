using Automation;
using Common;
using Common.Tests;
using Database.Tests;
using JetBrains.Annotations;
using SimulationEngineLib;
using Xunit;
using Xunit.Abstractions;

namespace SimulationEngine.Tests
{
    public class CreatePythonBindingsTest : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        public void RunPythonTestUtsp()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            const string bindings = @"C:\Work\utsp\lpgpythonbindings.py";
            const string data = @"C:\Work\utsp\lpgdata.py";
            PythonGenerator.MakeFullPythonBindings(db.ConnectionString, bindings,data);
        }

        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void RunPythonTestAutomatic()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass());
            const string bindings = @"lpgpythonbindings.py";
            const string data = @"lpgdata.py";
            PythonGenerator.MakeFullPythonBindings(db.ConnectionString, bindings, data);
        }
        public CreatePythonBindingsTest([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}

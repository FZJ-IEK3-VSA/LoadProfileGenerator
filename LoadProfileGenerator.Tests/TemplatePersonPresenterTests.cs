#region header

// // ProfileGenerator LoadProfileGenerator.Tests changed: 2016 10 12 09:40

#endregion

using System.Collections.ObjectModel;
using Common;
using Common.Tests;
using Database;
using Database.Tables.ModularHouseholds;
using Database.Tests;
using LoadProfileGenerator.Presenters.Households;
using NUnit.Framework;

namespace LoadProfileGenerator.Tests
{
    [TestFixture]
    public class TemplatePersonPresenterTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void Run()
        {
            DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass(), DatabaseSetup.TestPackage.LoadProfileGenerator);

            Simulator sim = new Simulator(db.ConnectionString);
            ObservableCollection<TemplatePersonPresenter.TraitPrio> traitPrios =
                new ObservableCollection<TemplatePersonPresenter.TraitPrio>();
            TemplatePerson template = sim.TemplatePersons.CreateNewItem(db.ConnectionString);
            template.SaveToDB();
            template.AddTrait(sim.HouseholdTraits.It[0]);
            TemplatePersonPresenter.RefreshTree(traitPrios, sim, template);
            Assert.That(traitPrios.Count > 0);
            db.Cleanup();
        }
    }
}
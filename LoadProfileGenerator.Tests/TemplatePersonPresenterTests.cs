﻿//#region header

//// // ProfileGenerator LoadProfileGenerator.Tests changed: 2016 10 12 09:40

//#endregion

//using System.Collections.ObjectModel;
//using Automation;
//using Common;
//using Common.Tests;
//using Database;
//using Database.Tables.ModularHouseholds;
//using Database.Tests;
//using FluentAssertions;
//using JetBrains.Annotations;
//using LoadProfileGenerator.Presenters.Households;

//using Xunit;
//using Xunit.Abstractions;


//namespace LoadProfileGenerator.Tests
//{
//    public class TemplatePersonPresenterTests : UnitTestBaseClass
//    {
//        [Fact]
//        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
//        public void Run()
//        {
//            using (DatabaseSetup db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
//            {
//                Simulator sim = new Simulator(db.ConnectionString);
//                ObservableCollection<TemplatePersonPresenter.TraitPrio> traitPrios =
//                    new ObservableCollection<TemplatePersonPresenter.TraitPrio>();
//                TemplatePerson template = sim.TemplatePersons.CreateNewItem(db.ConnectionString);
//                template.SaveToDB();
//                template.AddTrait(sim.HouseholdTraits.Items[0]);
//                TemplatePersonPresenter.RefreshTree(traitPrios, sim, template);
//                traitPrios.Count.Should().BeGreaterThan(0);
//                db.Cleanup();
//            }
//        }

//        public TemplatePersonPresenterTests([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//        {
//        }
//    }
//}
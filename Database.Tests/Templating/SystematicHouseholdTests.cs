using Automation;
using Common;
using Common.Tests;
using Xunit;
using Database.Templating;
using CalculationController.Integrity;
using Xunit.Abstractions;
using JetBrains.Annotations;
#pragma warning disable 8602
namespace Database.Tests.Templating {
public class SystematicHouseholdTests :UnitTestBaseClass {
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest0()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[0];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest1()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[1];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest2()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[2];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest3()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[3];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest4()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[4];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest5()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[5];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest6()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[6];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest7()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[7];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest8()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[8];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest9()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[9];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.SystematicSettlementTemplateTests)]
public void GenerateSettlementPreviewTest10()
{
    using (var db = new DatabaseSetup(Utili.GetCurrentMethodAndClass()))
    {
        var sim = new Simulator(db.ConnectionString);
        var template = sim.SettlementTemplates[10];
        template.DesiredHHCount = 50;
        var ste = new SettlementTemplateExecutor();
        ste.GenerateSettlementPreview(sim, template);
        ste.CreateSettlementFromPreview(sim, template);
        SimIntegrityChecker.Run(sim, CheckingOptions.Default());
        db.Cleanup();
    }
}
public SystematicHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
}}

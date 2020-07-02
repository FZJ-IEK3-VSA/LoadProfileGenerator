
using Automation;
using FluentAssertions;
using Xunit;

public class JsonCalcSpecTests {
    [Fact]
    public void CheckCopy()
    {
        JsonCalcSpecification jc = JsonCalcSpecification.MakeDefaultsForTesting();
        jc.EnableTransportation = true;
        var jc2 = new JsonCalcSpecification(jc);
        jc.Should().BeEquivalentTo(jc2);
    }
}
/*using System.IO;


namespace Common.Tests
{
    
    public class ChartLocalizerTests :UnitTestBaseClass
    {
        [Fact]
        public void RunTest()
        {
            WorkingDir wd = new WorkingDir(Utili.GetCurrentMethodAndClass());
#pragma warning disable S2696 // Instance members should not write to "static" fields
            ChartLocalizer.TranslationFileName = wd.Combine("translations.txt");
            ChartLocalizer.MissingFileName = wd.Combine("missing.txt");
#pragma warning restore S2696 // Instance members should not write to "static" fields
            using (StreamWriter sw = new StreamWriter(ChartLocalizer.TranslationFileName))
            {
                sw.WriteLine("Time;Zeit");
            }
            ChartLocalizer.ShouldTranslate = true;
            (ChartLocalizer.Get().GetTranslation("Time")).Should().Be("Zeit");
            wd.CleanUp();
        }
    }
}*/
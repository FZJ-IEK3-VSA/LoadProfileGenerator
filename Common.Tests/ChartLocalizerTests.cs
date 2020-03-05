/*using System.IO;
using NUnit.Framework;

namespace Common.Tests
{
    [TestFixture]
    public class ChartLocalizerTests :UnitTestBaseClass
    {
        [Test]
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
            Assert.AreEqual("Zeit", ChartLocalizer.Get().GetTranslation("Time"));
            wd.CleanUp();
        }
    }
}*/
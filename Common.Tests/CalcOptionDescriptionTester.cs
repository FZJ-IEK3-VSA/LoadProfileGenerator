using System;
using System.Collections.Generic;
using Automation;
using Automation.ResultFiles;
using Common.Enums;
using Xunit;
using Xunit.Abstractions;

namespace Common.Tests {
    public class CalcOptionDescriptionTester:UnitTestBaseClass {
        [Fact]
        [Trait(UnitTestCategories.Category, UnitTestCategories.BasicTest)]
        public void CheckIfAllCalcoptionsAreDescribed()
        {
            List<CalcOption> missing = new List<CalcOption>();
#pragma warning disable CS8605 // Unboxing a possibly null value.
            foreach (CalcOption co in Enum.GetValues(typeof(CalcOption))) {
#pragma warning restore CS8605 // Unboxing a possibly null value.
                if (!CalcOptionHelper.CalcOptionDictionary.ContainsKey(co)) {
                    missing.Add(co);
                }
            }
            List<CalcOption> stufftoignore = new List<CalcOption>();
            stufftoignore.Add(CalcOption.OverallDats);
            stufftoignore.Add(CalcOption.DetailedDatFiles);
            stufftoignore.Add(CalcOption.CalculationFlameChart);
            int count = 0;
            foreach (var option in missing) {
                if (stufftoignore.Contains(option)) {
                    continue;
                }
                Logger.Info("CalcOptionDictionary.Add(CalcOption." + option + ", \"\";");
                count++;
            }

            if (count > 0) {
                string s = String.Join(",",missing);
                throw new LPGException("Missing calc option descriptions:" +s);
            }
        }

        public CalcOptionDescriptionTester([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Tests;
using Xunit.Abstractions;

namespace Calculation.Tests {
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class CalcUnitTestBase : UnitTestBaseClass {
        [JetBrains.Annotations.NotNull]
        protected static CalcLoadType MakeCalcLoadType() =>
            new CalcLoadType("loadtype1", "Watt", "kWh", 1 / 1000.0, true, Guid.NewGuid().ToStrGuid());

        [JetBrains.Annotations.NotNull]
        protected static CalcProfile MakeCalcProfile5Min100()
        {
            var cp = new CalcProfile("5min100%", Guid.NewGuid().ToStrGuid(), new TimeSpan(0, 1, 0), ProfileType.Relative, "foo");
            cp.AddNewTimepoint(new TimeSpan(0, 0, 0), 1);
            cp.AddNewTimepoint(new TimeSpan(0, 5, 0), 0);
            cp.ConvertToTimesteps();
            return cp;
        }
        public CalcUnitTestBase([JetBrains.Annotations.NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Automation.ResultFiles;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Numeric;

using Xunit.Abstractions;

namespace Common.Tests {
    public static class FluentExtensions {
        public static AndConstraint<NumericAssertions<int>> BeApproximatelyWithinPercent(this NumericAssertions<int> parent, double target,
                                                                                         double percent, string because = "",
                                                                                         params object[] becauseArgs)
        {
            if (percent > 1) {
                throw new LPGException("PercentageRange > 1");
            }

            if (percent < 0) {
                throw new LPGException("PercentageRange<0");
            }

            // ReSharper disable once RedundantCast
            double actualValue =(int)parent.Subject;
            double minValue = target - target * percent;
            double maxValue = target + target * percent;

            FailIfValueOutsideBounds(minValue <= actualValue && actualValue <= maxValue, minValue, maxValue, actualValue, because, becauseArgs);

            return new AndConstraint<NumericAssertions<int>>(parent);
        }

        public static AndConstraint<NumericAssertions<double>> BeApproximatelyWithinPercent(this NumericAssertions<double> parent,
                                                                                            double target, double percent, string because = "",
                                                                                            params object[] becauseArgs)
        {
            if (percent > 1) {
                throw new LPGException("PercentageRange > 1");
            }

            if (percent < 0) {
                throw new LPGException("PercentageRange<0");
            }

            // ReSharper disable once RedundantCast
            double actualValue = (double)parent.Subject;
            double minValue = target - target * percent;
            double maxValue = target + target * percent;

            FailIfValueOutsideBounds(minValue <= actualValue && actualValue <= maxValue, minValue, maxValue, actualValue, because, becauseArgs);

            return new AndConstraint<NumericAssertions<double>>(parent);
        }

        public static AndConstraint<NumericAssertions<int>> BeApproximately(this NumericAssertions<int> parent, int target, double percent,
                                                                            string because = "", params object[] becauseArgs)
        {
            if (percent > 1) {
                throw new LPGException("PercentageRange > 1");
            }

            if (percent < 0) {
                throw new LPGException("PercentageRange<0");
            }

            // ReSharper disable once RedundantCast
            int actualValue = (int)parent.Subject;
            int minValue = (int)(target - target * percent);
            int maxValue = (int)(target + target * percent);

            FailIfValueOutsideBounds(minValue <= actualValue && actualValue <= maxValue, minValue, maxValue, actualValue, because, becauseArgs);

            return new AndConstraint<NumericAssertions<int>>(parent);
        }

        private static void FailIfValueOutsideBounds<TValue, TDelta>(bool valueWithinBounds, TValue minvalue, TDelta maxvalue, TValue actualValue,
                                                                     string because, object[] becauseArgs)
        {
            Execute.Assertion.ForCondition(valueWithinBounds).BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:value} to be within {0} - {1}{reason}, but found {2}.", minvalue, maxvalue, actualValue);
        }
    }



    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    public class UnitTestBaseClass {
        public ITestOutputHelper TestOutputHelper { get; }

        public UnitTestBaseClass(ITestOutputHelper testOutputHelper)
        {
            TestOutputHelper = testOutputHelper;
            Logger.Get().SetOutputHelper(testOutputHelper);
        }

        //private bool _skipEndCleaning;


        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "SetUp")]
        protected void SetUp()
        {
            var st = new StackTrace(1);
            var sf = st.GetFrame(0);
            var declaringType = sf?.GetMethod()?.DeclaringType;
            if (declaringType == null) {
                throw new LPGException("type was null.");
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            var msg = declaringType.FullName + "." + sf?.GetMethod()?.Name;
            var rnd = new Random();
            NormalRandom = new NormalRandom(0, 1, rnd);
            Logger.Info("locked by " + msg);

            Monitor.Enter(MyLock.Locker);
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TearDown")]
        protected void TearDown()
        {
            Monitor.Exit(MyLock.Locker);
            var st = new StackTrace(1);
            var sf = st.GetFrame(0);
            var type = sf?.GetMethod()?.DeclaringType;
            if (type == null) {
                throw new LPGException("Type was null.");
            }

            // ReSharper disable once ConstantConditionalAccessQualifier
            var msg = type.FullName + "." + sf?.GetMethod()?.Name;
            Logger.LogToFile = false;
            Logger.Info("unlocked by " + msg);
        }

        private static class MyLock {
            [JetBrains.Annotations.NotNull]
            public static object Locker { get; } = new object();
        }

        [JetBrains.Annotations.NotNull]
        protected NormalRandom NormalRandom { get; private set; } = new NormalRandom(0, 0.1, new Random());

    }
}

/*
[JetBrains.Annotations.NotNull]
protected static CalcPerson MakeCalcPerson([JetBrains.Annotations.NotNull] CalcLocation cloc, [JetBrains.Annotations.NotNull] CalcParameters calcParameters, [JetBrains.Annotations.NotNull] ILogFile lf)
{
    BitArray isSick = new BitArray(calcParameters.InternalTimesteps);
    BitArray isOnVacation = new BitArray(calcParameters.InternalTimesteps);
    CalcPersonDto pdto = CalcPersonDto.MakeExamplePerson();
    //calcpersonname", 1, 1, new Random(), 1, PermittedGender.Male, null,
    //"hh-6", cloc, "traittag", "testhh", calcParameters,isSick, Guid.NewGuid().ToStrGuid()
    return new CalcPerson(pdto,new Random(), lf,cloc,calcParameters,isSick,isOnVacation);
}*


}
}*/

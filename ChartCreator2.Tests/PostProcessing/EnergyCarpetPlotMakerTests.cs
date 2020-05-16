using System;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace ChartCreator2.Tests.PostProcessing {

    public class EnergyCarpetPlotMakerTests : UnitTestBaseClass
    {
        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.ManualOnly)]
        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        public void MakeCarpetTest()
        {
            //TODO: fix this permanently
            throw new NotImplementedException();
            /*
#pragma warning disable 162
            CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults();
            calcParameters.SetTransportMode(false);
            var clt = new CalcLoadType("Electricity", 1, "kW", "kWh", 1, true, Guid.NewGuid().ToStrGuid());
            List<double> values;
            int count;
            using (
                var sr =
                    new StreamReader(@"X:\hh50_led\32_(bulletin100001)Hous\Results\DeviceProfiles.Electricity.csv")) {
                var header = sr.ReadLine();
                if (header == null) {
                    throw new LPGException("Readline failed");
                }
                var headerarr = header.Split(';');
                Logger.Info(headerarr[6]);
                values = new List<double>();
                count = 0;
                while (!sr.EndOfStream) {
                    var s = sr.ReadLine();
                    if (s == null) {
                        throw new LPGException("Readline failed");
                    }
                    var arr = s.Split(';');
                    var val = Convert.ToDouble(arr[6], CultureInfo.CurrentCulture) * 60000;
                    values.Add(val);
                    count++;
                }
            }
            using (
                var fs = new FileStream(@"X:\hh50_led\32_(bulletin100001)Hous\Results\carpet.png",
                    FileMode.Create)) {
                var totalDuration = TimeSpan.FromMinutes(count);
                //calcParameters.InitializeTimeSteps(new DateTime(2018,1,1),new DateTime(2018,1,2 ),new TimeSpan(0,1,0),1,false  );
                //EnergyCarpetPlotMaker ecp = new EnergyCarpetPlotMaker(calcParameters);
                //ecp.MakeCarpet(fs, values.ToArray(), totalDuration, clt.Name, 5,"Watt");
            }
#pragma warning restore 162*/
        }

        public EnergyCarpetPlotMakerTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }
    }
}
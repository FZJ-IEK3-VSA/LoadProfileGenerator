//-----------------------------------------------------------------------

// <copyright>
//
// Copyright (c) TU Chemnitz, Prof. Technische Thermodynamik
// Written by Noah Pflugradt.
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//  Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the distribution.
//  All advertising materials mentioning features or use of this software must display the following acknowledgement:
//  “This product includes software developed by the TU Chemnitz, Prof. Technische Thermodynamik and its contributors.”
//  Neither the name of the University nor the names of its contributors may be used to endorse or promote products
//  derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE UNIVERSITY 'AS IS' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE UNIVERSITY OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, S
// PECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using Automation;
using Automation.ResultFiles;
using CalculationController.DtoFactories;
using CalculationEngine.OnlineLogging;
using Common;
using Common.JSON;
using Common.SQLResultLogging.InputLoggers;
using Common.Tests;
using JetBrains.Annotations;

using Xunit;
using Xunit.Abstractions;

namespace Calculation.Tests.Logfile
{
    public class LogFileTests : UnitTestBaseClass
    {
        public LogFileTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        [Trait(UnitTestCategories.Category,UnitTestCategories.BasicTest)]
        public void BasicTest()
        {
            using (WorkingDir wd = new WorkingDir("BasicLogfileTest"))
            {
                CalcParameters calcParameters = CalcParametersFactory.MakeGoodDefaults().EnableShowSettlingPeriod();
                wd.InputDataLogger.AddSaver(new HouseholdKeyLogger(wd.SqlResultLoggingService));
                wd.InputDataLogger.AddSaver(new ResultFileEntryLogger(wd.SqlResultLoggingService));
                using (FileFactoryAndTracker fft = new FileFactoryAndTracker(wd.WorkingDirectory, "hh1", wd.InputDataLogger))
                {
                    fft.RegisterHousehold(Constants.GeneralHouseholdKey, "general", HouseholdKeyType.General, "desc", null, null);
                    //SqlResultLoggingService srls = new SqlResultLoggingService(wd.WorkingDirectory);
                    using (LogFile lf = new LogFile(calcParameters, fft))
                    {
                        lf.Dispose();
                    }
                }
                wd.CleanUp();
            }
        }
    }
}
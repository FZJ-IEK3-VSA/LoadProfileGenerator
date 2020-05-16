/*ing System;
using System.Collections;
using System.Collections.Generic;
using Calculation.HouseholdElements;
using CommonDataWPF;*/

using Common.Tests;
using JetBrains.Annotations;
using Xunit.Abstractions;

namespace Calculation.Tests.HouseholdElements
{

    public class CalcDeviceTests : UnitTestBaseClass
    {
        /*   private static void CompareResults(int start, int end, BitArray br, List<BusyEntry> entries)
           {
               bool result1 = IsbusyCheck(start, end, br);
   //            bool result2 = CalcDevice.HasBusyEntryDuringTimespan(start, end, entries);
               Logger.Info("Test from start " + start + " to " + end + " real result:" + result1 + " quick result " +
                                 result2);
               (result2).Should().Be(result1);
           }

           private static bool IsbusyCheck(int startidx, int duration, BitArray br)
           {
               for (int i = startidx; (i < (startidx + duration)) && (i < br.Length); i++)
                   if (br[i])
                       return true;
               return false;
           }

           [Fact]
           public void IsBusyDuringTimespanTest2ByteFirstbusy()
           {
               BitArray br = new BitArray(2*4*8);
               br[31] = true;
               List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
               (entrys.Count).Should().Be(1);
               CompareResults(0, 64, br, entrys);
               CompareResults(0, 32, br, entrys);
               CompareResults(33, 64, br, entrys);
           }

           [Fact]
           public void IsBusyDuringTimespanTest4Byte2Ndbusy()
           {
               BitArray br = new BitArray(4*4*8);
               br[32] = true;
               List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
               (entrys.Count).Should().Be(1);
               CompareResults(0, 128, br, entrys);
               CompareResults(0, 31, br, entrys);
               CompareResults(16, 40, br, entrys);
               CompareResults(16, 128, br, entrys);
               CompareResults(33, 128, br, entrys);
               CompareResults(64, 96, br, entrys);
           }

           [Fact]
           public void IsBusyDuringTimespanTest4ByteFirstbusy()
           {
               BitArray br = new BitArray(4*4*8);

               br[0] = true;
               List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
               (entrys.Count).Should().Be(1);
               CompareResults(0, 128, br, entrys);
               CompareResults(0, 31, br, entrys);
               CompareResults(16, 40, br, entrys);
               CompareResults(16, 128, br, entrys);
               CompareResults(33, 128, br, entrys);
               CompareResults(64, 96, br, entrys);
           }

           [Fact]
           public void IsBusyDuringTimespanTest4ByteLastbusy()
           {
               BitArray br = new BitArray(4*4*8);
               br[127] = true;
               List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
               (entrys.Count).Should().Be(1);
               CompareResults(0, 128, br, entrys);
               CompareResults(0, 31, br, entrys);
               CompareResults(16, 40, br, entrys);
               CompareResults(16, 128, br, entrys);
               CompareResults(64, 96, br, entrys);
           }
           */

            /*
        [Fact]
           public void IsBusyDuringTimespanTestFirst()
           {
               BitArray br = new BitArray(10);
               br[0] = true;
               List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
               (entrys.Count).Should().Be(1);
               BusyEntry be = entrys[0];
               (be.Start).Should().Be(0);
               (be.End).Should().Be(0);
               Assert.True(CalcDevice.HasBusyEntryDuringTimespan(0, 1, entrys));

               Assert.True(CalcDevice.HasBusyEntryDuringTimespan(0, 10, entrys));
               Assert.False(CalcDevice.HasBusyEntryDuringTimespan(1, 1, entrys));
               for (int i = 1; i < br.Length; i++)
               {
                   Assert.False(CalcDevice.HasBusyEntryDuringTimespan(i, 1, entrys));
                   Assert.False(CalcDevice.HasBusyEntryDuringTimespan(i, 10, entrys));
               }
           }

        [Fact]
        public void IsBusyDuringTimespanTestLast()
        {
            BitArray br = new BitArray(10);
            br[9] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(1);
            CompareResults(0, 9, br, entrys);
        }

        [Fact]
        public void IsBusyDuringTimespanTestLastBusy()
        {
            BitArray br = new BitArray(10);
            br[9] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(1);
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(9, 1, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(9, 10, entrys));
            for (int i = 0; i < br.Length; i++)
                if (i != 9)
                    Assert.False(CalcDevice.HasBusyEntryDuringTimespan(i, 1, entrys));
        }

        [Fact]
        public void IsBusyDuringTimespanTestMiddle()
        {
            BitArray br = new BitArray(10);
            br[5] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(1);

            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(4, 1, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(5, 1, entrys));
            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(6, 1, entrys));
        }

        [Fact]
        public void IsBusyDuringTimespanTestMiddle2()
        {
            BitArray br = new BitArray(10);
            br[1] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(1);

            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(0, 1, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(1, 1, entrys));
            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(0, 1, entrys));
        }

        [Fact]
        public void IsBusyDuringTimespanTestMiddle3()
        {
            BitArray br = new BitArray(10);
            br[5] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);

            (entrys.Count).Should().Be(1);
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(4, 2, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(5, 2, entrys));
            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(6, 2, entrys));
        }

        [Fact]
        public void IsBusyDuringTimespanTestMiddle4()
        {
            BitArray br = new BitArray(10);
            for (int i = 0; i < 3; i++)
                br[i] = true;
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(1);
            (entrys[0].Start).Should().Be(0);
            (entrys[0].End).Should().Be(2);

            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(0, 1, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(0, 2, entrys));
            Assert.True(CalcDevice.HasBusyEntryDuringTimespan(0, 5, entrys));
            Assert.False(CalcDevice.HasBusyEntryDuringTimespan(3, 1, entrys));
        }

        [Fact]
        public void IsBusyDuringTimespanTestNone()
        {
            BitArray br = new BitArray(10);
            List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
            (entrys.Count).Should().Be(0);
            for (int i = 0; i < br.Length; i++)
            {
                Assert.False(CalcDevice.HasBusyEntryDuringTimespan(i, 1, entrys));
                Assert.False(CalcDevice.HasBusyEntryDuringTimespan(i, 10, entrys));
            }
        }

        [Fact]
        public void IsBusyRandomTimespans()
        {
            Random r = new Random(1);
            for (int i = 0; i < 100; i++)
            {
                BitArray br = new BitArray(1000);
                int busyCount = r.Next(100);
                for (int j = 0; j < busyCount; j++)
                {
                    int start = r.Next(1000);
                    int duration = r.Next(1000 - start);
                    for (int k = 0; k < duration; k++)
                        br[start + duration] = true;
                }
                List<BusyEntry> entrys = CalcDevice.MakeBusyEntries(br);
                for (int j = 0; j < 1000; j++)
                {
                    int duration = r.Next(1000 - j);
                    CompareResults(j, j + duration, br, entrys);
                }
            }
        }*/
            public CalcDeviceTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
            {
            }
    }
}
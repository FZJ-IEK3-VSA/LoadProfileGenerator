using Automation;
using Automation.ResultFiles;
using Xunit;
using Common.Tests;
using Xunit.Abstractions;
using JetBrains.Annotations;
#pragma warning disable 8602
namespace SimulationEngine.Tests {
public class SystematicHouseholdTests :UnitTestBaseClass {

[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest0(){
      const int hhnumer = 0;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest1(){
      const int hhnumer = 1;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest2(){
      const int hhnumer = 2;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest3(){
      const int hhnumer = 3;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest4(){
      const int hhnumer = 4;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest5(){
      const int hhnumer = 5;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest6(){
      const int hhnumer = 6;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest7(){
      const int hhnumer = 7;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest8(){
      const int hhnumer = 8;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest9(){
      const int hhnumer = 9;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest10(){
      const int hhnumer = 10;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest11(){
      const int hhnumer = 11;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest12(){
      const int hhnumer = 12;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest13(){
      const int hhnumer = 13;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest14(){
      const int hhnumer = 14;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest15(){
      const int hhnumer = 15;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest16(){
      const int hhnumer = 16;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest17(){
      const int hhnumer = 17;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest18(){
      const int hhnumer = 18;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest19(){
      const int hhnumer = 19;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest20(){
      const int hhnumer = 20;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest21(){
      const int hhnumer = 21;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest22(){
      const int hhnumer = 22;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest23(){
      const int hhnumer = 23;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest24(){
      const int hhnumer = 24;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest25(){
      const int hhnumer = 25;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest26(){
      const int hhnumer = 26;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest27(){
      const int hhnumer = 27;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest28(){
      const int hhnumer = 28;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest29(){
      const int hhnumer = 29;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest30(){
      const int hhnumer = 30;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest31(){
      const int hhnumer = 31;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest32(){
      const int hhnumer = 32;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest33(){
      const int hhnumer = 33;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest34(){
      const int hhnumer = 34;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest35(){
      const int hhnumer = 35;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest36(){
      const int hhnumer = 36;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest37(){
      const int hhnumer = 37;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest38(){
      const int hhnumer = 38;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest39(){
      const int hhnumer = 39;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest40(){
      const int hhnumer = 40;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest41(){
      const int hhnumer = 41;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest42(){
      const int hhnumer = 42;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest43(){
      const int hhnumer = 43;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest44(){
      const int hhnumer = 44;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest45(){
      const int hhnumer = 45;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest46(){
      const int hhnumer = 46;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest47(){
      const int hhnumer = 47;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest48(){
      const int hhnumer = 48;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest49(){
      const int hhnumer = 49;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest50(){
      const int hhnumer = 50;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest51(){
      const int hhnumer = 51;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest52(){
      const int hhnumer = 52;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest53(){
      const int hhnumer = 53;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest54(){
      const int hhnumer = 54;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest55(){
      const int hhnumer = 55;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest56(){
      const int hhnumer = 56;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest57(){
      const int hhnumer = 57;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest58(){
      const int hhnumer = 58;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest59(){
      const int hhnumer = 59;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest60(){
      const int hhnumer = 60;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest61(){
      const int hhnumer = 61;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest62(){
      const int hhnumer = 62;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest63(){
      const int hhnumer = 63;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest64(){
      const int hhnumer = 64;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest65(){
      const int hhnumer = 65;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest66(){
      const int hhnumer = 66;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest67(){
      const int hhnumer = 67;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest68(){
      const int hhnumer = 68;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest69(){
      const int hhnumer = 69;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest70(){
      const int hhnumer = 70;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest71(){
      const int hhnumer = 71;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest72(){
      const int hhnumer = 72;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest73(){
      const int hhnumer = 73;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest74(){
      const int hhnumer = 74;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest75(){
      const int hhnumer = 75;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest76(){
      const int hhnumer = 76;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest77(){
      const int hhnumer = 77;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest78(){
      const int hhnumer = 78;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest79(){
      const int hhnumer = 79;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest80(){
      const int hhnumer = 80;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest81(){
      const int hhnumer = 81;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest82(){
      const int hhnumer = 82;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest83(){
      const int hhnumer = 83;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest84(){
      const int hhnumer = 84;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest85(){
      const int hhnumer = 85;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest86(){
      const int hhnumer = 86;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest87(){
      const int hhnumer = 87;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest88(){
      const int hhnumer = 88;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest89(){
      const int hhnumer = 89;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest90(){
      const int hhnumer = 90;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest91(){
      const int hhnumer = 91;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest92(){
      const int hhnumer = 92;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest93(){
      const int hhnumer = 93;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest94(){
      const int hhnumer = 94;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest95(){
      const int hhnumer = 95;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest96(){
      const int hhnumer = 96;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest97(){
      const int hhnumer = 97;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest98(){
      const int hhnumer = 98;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest99(){
      const int hhnumer = 99;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest100(){
      const int hhnumer = 100;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest101(){
      const int hhnumer = 101;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest102(){
      const int hhnumer = 102;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest103(){
      const int hhnumer = 103;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest104(){
      const int hhnumer = 104;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest105(){
      const int hhnumer = 105;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest106(){
      const int hhnumer = 106;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest107(){
      const int hhnumer = 107;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest108(){
      const int hhnumer = 108;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest109(){
      const int hhnumer = 109;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest110(){
      const int hhnumer = 110;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest111(){
      const int hhnumer = 111;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest112(){
      const int hhnumer = 112;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest113(){
      const int hhnumer = 113;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest114(){
      const int hhnumer = 114;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest115(){
      const int hhnumer = 115;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest116(){
      const int hhnumer = 116;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest117(){
      const int hhnumer = 117;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest118(){
      const int hhnumer = 118;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest119(){
      const int hhnumer = 119;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest120(){
      const int hhnumer = 120;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest121(){
      const int hhnumer = 121;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest122(){
      const int hhnumer = 122;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest123(){
      const int hhnumer = 123;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest124(){
      const int hhnumer = 124;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest125(){
      const int hhnumer = 125;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest126(){
      const int hhnumer = 126;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest127(){
      const int hhnumer = 127;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest128(){
      const int hhnumer = 128;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest129(){
      const int hhnumer = 129;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest130(){
      const int hhnumer = 130;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest131(){
      const int hhnumer = 131;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest132(){
      const int hhnumer = 132;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest133(){
      const int hhnumer = 133;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest134(){
      const int hhnumer = 134;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest135(){
      const int hhnumer = 135;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest136(){
      const int hhnumer = 136;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest137(){
      const int hhnumer = 137;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest138(){
      const int hhnumer = 138;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest139(){
      const int hhnumer = 139;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest140(){
      const int hhnumer = 140;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest141(){
      const int hhnumer = 141;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest142(){
      const int hhnumer = 142;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest143(){
      const int hhnumer = 143;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest144(){
      const int hhnumer = 144;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest145(){
      const int hhnumer = 145;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest146(){
      const int hhnumer = 146;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest147(){
      const int hhnumer = 147;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest148(){
      const int hhnumer = 148;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest149(){
      const int hhnumer = 149;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest150(){
      const int hhnumer = 150;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest151(){
      const int hhnumer = 151;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest152(){
      const int hhnumer = 152;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest153(){
      const int hhnumer = 153;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest154(){
      const int hhnumer = 154;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest155(){
      const int hhnumer = 155;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest156(){
      const int hhnumer = 156;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest157(){
      const int hhnumer = 157;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest158(){
      const int hhnumer = 158;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest159(){
      const int hhnumer = 159;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest160(){
      const int hhnumer = 160;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest161(){
      const int hhnumer = 161;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest162(){
      const int hhnumer = 162;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest163(){
      const int hhnumer = 163;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest164(){
      const int hhnumer = 164;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest165(){
      const int hhnumer = 165;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest166(){
      const int hhnumer = 166;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest167(){
      const int hhnumer = 167;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest168(){
      const int hhnumer = 168;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest169(){
      const int hhnumer = 169;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest170(){
      const int hhnumer = 170;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest171(){
      const int hhnumer = 171;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest172(){
      const int hhnumer = 172;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest173(){
      const int hhnumer = 173;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest174(){
      const int hhnumer = 174;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest175(){
      const int hhnumer = 175;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest176(){
      const int hhnumer = 176;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest177(){
      const int hhnumer = 177;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest178(){
      const int hhnumer = 178;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest179(){
      const int hhnumer = 179;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest180(){
      const int hhnumer = 180;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest181(){
      const int hhnumer = 181;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest182(){
      const int hhnumer = 182;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest183(){
      const int hhnumer = 183;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest184(){
      const int hhnumer = 184;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest185(){
      const int hhnumer = 185;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest186(){
      const int hhnumer = 186;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest187(){
      const int hhnumer = 187;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest188(){
      const int hhnumer = 188;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest189(){
      const int hhnumer = 189;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest190(){
      const int hhnumer = 190;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest191(){
      const int hhnumer = 191;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest192(){
      const int hhnumer = 192;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest193(){
      const int hhnumer = 193;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest194(){
      const int hhnumer = 194;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest195(){
      const int hhnumer = 195;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest196(){
      const int hhnumer = 196;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest197(){
      const int hhnumer = 197;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest198(){
      const int hhnumer = 198;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest199(){
      const int hhnumer = 199;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest200(){
      const int hhnumer = 200;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest201(){
      const int hhnumer = 201;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest202(){
      const int hhnumer = 202;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest203(){
      const int hhnumer = 203;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest204(){
      const int hhnumer = 204;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest205(){
      const int hhnumer = 205;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest206(){
      const int hhnumer = 206;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest207(){
      const int hhnumer = 207;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest208(){
      const int hhnumer = 208;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest209(){
      const int hhnumer = 209;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest210(){
      const int hhnumer = 210;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest211(){
      const int hhnumer = 211;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest212(){
      const int hhnumer = 212;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest213(){
      const int hhnumer = 213;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest214(){
      const int hhnumer = 214;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest215(){
      const int hhnumer = 215;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest216(){
      const int hhnumer = 216;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest217(){
      const int hhnumer = 217;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest218(){
      const int hhnumer = 218;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest219(){
      const int hhnumer = 219;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest220(){
      const int hhnumer = 220;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest221(){
      const int hhnumer = 221;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest222(){
      const int hhnumer = 222;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest223(){
      const int hhnumer = 223;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest224(){
      const int hhnumer = 224;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest225(){
      const int hhnumer = 225;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest226(){
      const int hhnumer = 226;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest227(){
      const int hhnumer = 227;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest228(){
      const int hhnumer = 228;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest229(){
      const int hhnumer = 229;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest230(){
      const int hhnumer = 230;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest231(){
      const int hhnumer = 231;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest232(){
      const int hhnumer = 232;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest233(){
      const int hhnumer = 233;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest234(){
      const int hhnumer = 234;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest235(){
      const int hhnumer = 235;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest236(){
      const int hhnumer = 236;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest237(){
      const int hhnumer = 237;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest238(){
      const int hhnumer = 238;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest239(){
      const int hhnumer = 239;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest240(){
      const int hhnumer = 240;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest241(){
      const int hhnumer = 241;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest242(){
      const int hhnumer = 242;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest243(){
      const int hhnumer = 243;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest244(){
      const int hhnumer = 244;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest245(){
      const int hhnumer = 245;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest246(){
      const int hhnumer = 246;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest247(){
      const int hhnumer = 247;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest248(){
      const int hhnumer = 248;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest249(){
      const int hhnumer = 249;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest250(){
      const int hhnumer = 250;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest251(){
      const int hhnumer = 251;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest252(){
      const int hhnumer = 252;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest253(){
      const int hhnumer = 253;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest254(){
      const int hhnumer = 254;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest255(){
      const int hhnumer = 255;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest256(){
      const int hhnumer = 256;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest257(){
      const int hhnumer = 257;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest258(){
      const int hhnumer = 258;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest259(){
      const int hhnumer = 259;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest260(){
      const int hhnumer = 260;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest261(){
      const int hhnumer = 261;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest262(){
      const int hhnumer = 262;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest263(){
      const int hhnumer = 263;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest264(){
      const int hhnumer = 264;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest265(){
      const int hhnumer = 265;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest266(){
      const int hhnumer = 266;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest267(){
      const int hhnumer = 267;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest268(){
      const int hhnumer = 268;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest269(){
      const int hhnumer = 269;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest270(){
      const int hhnumer = 270;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest271(){
      const int hhnumer = 271;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest272(){
      const int hhnumer = 272;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest273(){
      const int hhnumer = 273;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest274(){
      const int hhnumer = 274;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest275(){
      const int hhnumer = 275;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest276(){
      const int hhnumer = 276;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest277(){
      const int hhnumer = 277;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest278(){
      const int hhnumer = 278;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest279(){
      const int hhnumer = 279;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest280(){
      const int hhnumer = 280;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest281(){
      const int hhnumer = 281;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest282(){
      const int hhnumer = 282;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest283(){
      const int hhnumer = 283;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest284(){
      const int hhnumer = 284;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest285(){
      const int hhnumer = 285;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest286(){
      const int hhnumer = 286;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest287(){
      const int hhnumer = 287;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest288(){
      const int hhnumer = 288;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest289(){
      const int hhnumer = 289;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest290(){
      const int hhnumer = 290;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest291(){
      const int hhnumer = 291;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest292(){
      const int hhnumer = 292;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest293(){
      const int hhnumer = 293;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest294(){
      const int hhnumer = 294;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest295(){
      const int hhnumer = 295;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest296(){
      const int hhnumer = 296;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest297(){
      const int hhnumer = 297;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest298(){
      const int hhnumer = 298;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest299(){
      const int hhnumer = 299;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest300(){
      const int hhnumer = 300;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest301(){
      const int hhnumer = 301;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest302(){
      const int hhnumer = 302;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest303(){
      const int hhnumer = 303;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest304(){
      const int hhnumer = 304;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest305(){
      const int hhnumer = 305;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest306(){
      const int hhnumer = 306;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest307(){
      const int hhnumer = 307;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest308(){
      const int hhnumer = 308;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest309(){
      const int hhnumer = 309;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest310(){
      const int hhnumer = 310;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest311(){
      const int hhnumer = 311;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest312(){
      const int hhnumer = 312;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest313(){
      const int hhnumer = 313;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest314(){
      const int hhnumer = 314;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest315(){
      const int hhnumer = 315;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest316(){
      const int hhnumer = 316;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest317(){
      const int hhnumer = 317;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest318(){
      const int hhnumer = 318;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest319(){
      const int hhnumer = 319;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest320(){
      const int hhnumer = 320;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest321(){
      const int hhnumer = 321;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest322(){
      const int hhnumer = 322;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest323(){
      const int hhnumer = 323;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest324(){
      const int hhnumer = 324;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest325(){
      const int hhnumer = 325;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest326(){
      const int hhnumer = 326;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest327(){
      const int hhnumer = 327;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest328(){
      const int hhnumer = 328;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest329(){
      const int hhnumer = 329;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest330(){
      const int hhnumer = 330;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest331(){
      const int hhnumer = 331;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest332(){
      const int hhnumer = 332;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest333(){
      const int hhnumer = 333;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest334(){
      const int hhnumer = 334;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest335(){
      const int hhnumer = 335;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest336(){
      const int hhnumer = 336;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest337(){
      const int hhnumer = 337;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest338(){
      const int hhnumer = 338;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest339(){
      const int hhnumer = 339;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest340(){
      const int hhnumer = 340;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest341(){
      const int hhnumer = 341;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest342(){
      const int hhnumer = 342;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest343(){
      const int hhnumer = 343;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest344(){
      const int hhnumer = 344;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest345(){
      const int hhnumer = 345;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest346(){
      const int hhnumer = 346;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest347(){
      const int hhnumer = 347;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest348(){
      const int hhnumer = 348;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest349(){
      const int hhnumer = 349;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest350(){
      const int hhnumer = 350;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest351(){
      const int hhnumer = 351;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest352(){
      const int hhnumer = 352;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest353(){
      const int hhnumer = 353;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest354(){
      const int hhnumer = 354;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest355(){
      const int hhnumer = 355;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest356(){
      const int hhnumer = 356;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest357(){
      const int hhnumer = 357;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest358(){
      const int hhnumer = 358;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest359(){
      const int hhnumer = 359;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest360(){
      const int hhnumer = 360;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest361(){
      const int hhnumer = 361;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest362(){
      const int hhnumer = 362;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest363(){
      const int hhnumer = 363;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest364(){
      const int hhnumer = 364;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest365(){
      const int hhnumer = 365;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest366(){
      const int hhnumer = 366;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest367(){
      const int hhnumer = 367;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest368(){
      const int hhnumer = 368;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest369(){
      const int hhnumer = 369;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest370(){
      const int hhnumer = 370;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest371(){
      const int hhnumer = 371;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest372(){
      const int hhnumer = 372;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest373(){
      const int hhnumer = 373;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest374(){
      const int hhnumer = 374;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest375(){
      const int hhnumer = 375;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest376(){
      const int hhnumer = 376;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest377(){
      const int hhnumer = 377;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest378(){
      const int hhnumer = 378;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest379(){
      const int hhnumer = 379;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest380(){
      const int hhnumer = 380;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest381(){
      const int hhnumer = 381;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest382(){
      const int hhnumer = 382;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest383(){
      const int hhnumer = 383;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest384(){
      const int hhnumer = 384;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest385(){
      const int hhnumer = 385;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest386(){
      const int hhnumer = 386;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest387(){
      const int hhnumer = 387;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest388(){
      const int hhnumer = 388;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest389(){
      const int hhnumer = 389;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest390(){
      const int hhnumer = 390;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest391(){
      const int hhnumer = 391;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest392(){
      const int hhnumer = 392;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest393(){
      const int hhnumer = 393;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest394(){
      const int hhnumer = 394;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest395(){
      const int hhnumer = 395;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest396(){
      const int hhnumer = 396;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest397(){
      const int hhnumer = 397;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest398(){
      const int hhnumer = 398;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest399(){
      const int hhnumer = 399;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest400(){
      const int hhnumer = 400;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest401(){
      const int hhnumer = 401;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest402(){
      const int hhnumer = 402;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest403(){
      const int hhnumer = 403;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest404(){
      const int hhnumer = 404;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest405(){
      const int hhnumer = 405;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest406(){
      const int hhnumer = 406;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest407(){
      const int hhnumer = 407;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest408(){
      const int hhnumer = 408;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest409(){
      const int hhnumer = 409;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest410(){
      const int hhnumer = 410;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest411(){
      const int hhnumer = 411;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest412(){
      const int hhnumer = 412;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest413(){
      const int hhnumer = 413;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}


[Fact]
[Trait(UnitTestCategories.Category, UnitTestCategories.LongTest7)]
public void TestHouseholdTest414(){
      const int hhnumer = 414;
      HouseJobTestHelper.RunSingleHouse(sim => {
      var hj = HouseJobCalcPreparer.PrepareNewHouseForHouseholdTesting(sim,sim.ModularHouseholds[hhnumer]);
      if (hj.CalcSpec?.CalcOptions == null) { throw new LPGException(); }
      hj.CalcSpec.DefaultForOutputFiles = OutputFileDefault.Reasonable;
return hj; }, x => {});
}

public SystematicHouseholdTests([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
}}

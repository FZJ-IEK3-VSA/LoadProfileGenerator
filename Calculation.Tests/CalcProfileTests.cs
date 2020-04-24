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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Automation;
using CalculationEngine.HouseholdElements;
using Common;
using Common.CalcDto;
using Common.Tests;
using NUnit.Framework;

namespace Calculation.Tests {
    [TestFixture]
    public class CalcProfileTests : UnitTestBaseClass
    {
        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CompressExpandDoubleArrayExpand1ShouldWork()
        {
            var tmparr = new List<double>();
            tmparr.AddRange(new double[] {1, 2, 3});

            var cp = new CalcProfile("bla", Guid.NewGuid().ToString(), tmparr,  ProfileType.Absolute,
                "bla");
            var result = cp.CompressExpandDoubleArray(2);
            //List<double> result = CalcProfile.CompressExpandDoubleArray(tmparr, 2);
            Assert.AreEqual(6, result.StepValues.Count);
            Assert.AreEqual(1, result.StepValues[0]);
            Assert.AreEqual(1, result.StepValues[1]);
            Assert.AreEqual(2, result.StepValues[2]);
            Assert.AreEqual(2, result.StepValues[3]);
            Assert.AreEqual(3, result.StepValues[4]);
            Assert.AreEqual(3, result.StepValues[5]);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CompressExpandDoubleArrayExpand2ShouldWork()
        {
            var tmparr = new List<double>();
            tmparr.AddRange(new double[] {1, 2, 3});
            var cp = new CalcProfile("bla", Guid.NewGuid().ToString(), tmparr, ProfileType.Absolute, "bla");
            var result = cp.CompressExpandDoubleArray(1.5);
            //List<double> result = CalcProfile.CompressExpandDoubleArray(tmparr, 1.5);
            Assert.AreEqual(5, result.StepValues.Count);
            Assert.AreEqual(1, result.StepValues[0]);
            Assert.AreEqual(2, result.StepValues[1]);
            Assert.AreEqual(2, result.StepValues[2]);
            Assert.AreEqual(3, result.StepValues[3]);
            Assert.AreEqual(3, result.StepValues[4]);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public static void CompressExpandDoubleArrayExpand3ShouldWork()
        {
            double[] tmparr = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            //List<double> tmplist = new List<double>(tmparr);
            //List<double> result = CalcProfile.CompressExpandDoubleArray(tmplist, 1.1);
            var cp = new CalcProfile("bla", Guid.NewGuid().ToString(), tmparr.ToList(), ProfileType.Absolute,
                "bla");
            var result = cp.CompressExpandDoubleArray(1.5);
            foreach (double resultStepValue in result.StepValues) {
                Logger.Info(resultStepValue.ToString(CultureInfo.InvariantCulture));
            }
            Assert.AreEqual(15, result.StepValues.Count);
            Assert.AreEqual(0, result.StepValues[0]);
            Assert.AreEqual(1, result.StepValues[1]);
            Assert.AreEqual(1, result.StepValues[2]);
            Assert.AreEqual(2, result.StepValues[3]);
            Assert.AreEqual(3, result.StepValues[4]);
            Assert.AreEqual(3, result.StepValues[5]);
            Assert.AreEqual(4, result.StepValues[6]);
            Assert.AreEqual(5, result.StepValues[7]);
            Assert.AreEqual(5, result.StepValues[8]);
            Assert.AreEqual(6, result.StepValues[9]);
            Assert.AreEqual(7, result.StepValues[10]);
            Assert.AreEqual(7, result.StepValues[11]);
            Assert.AreEqual(8, result.StepValues[12]);
            Assert.AreEqual(9, result.StepValues[13]);
            Assert.AreEqual(9, result.StepValues[14]);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CompressExpandDoubleArrayShrink1ShouldWork()
        {
            double[] tmparr = {1, 2, 3, 4};
            var cp = new CalcProfile("bla", Guid.NewGuid().ToString(), tmparr.ToList(), ProfileType.Absolute,
                "bla");
            var result = cp.CompressExpandDoubleArray(0.5);
            //List<double> result = CalcProfile.CompressExpandDoubleArray(tmplist, 0.5);
            Assert.AreEqual(2, result.StepValues.Count);
            Assert.AreEqual(2, result.StepValues[0]);
            Assert.AreEqual(4, result.StepValues[1]);
        }

        [Test]
        [Category(UnitTestCategories.BasicTest)]
        public void CompressExpandDoubleArrayShrink2ShouldWork()
        {
            double[] tmparr = {1, 2, 3, 4, 5};
            //List<double> tmplist = new List<double>(tmparr);
            //List<double> result = CalcProfile.CompressExpandDoubleArray(tmplist, 0.5);
            var cp = new CalcProfile("bla", Guid.NewGuid().ToString(), tmparr.ToList(),ProfileType.Absolute,
                "bla");
            var result = cp.CompressExpandDoubleArray(0.5);
            Assert.AreEqual(3, result.StepValues.Count);
            Assert.AreEqual(2, result.StepValues[0]);
            Assert.AreEqual(4, result.StepValues[1]);
            Assert.AreEqual(5, result.StepValues[2]);
        }
    }
}
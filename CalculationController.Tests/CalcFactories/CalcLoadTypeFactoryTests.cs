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
using System.Collections.ObjectModel;
using Automation;
using CalculationController.CalcFactories;
using Common.Tests;
using Database.Tables.BasicHouseholds;
using NUnit.Framework;

namespace CalculationController.Tests.CalcFactories
{
    [TestFixture]
    public class CalcLoadTypeFactoryTests : UnitTestBaseClass
    {
        [Test]
        [Category("BasicTest")]
        public void MakeLoadTypesTest()
        {
            //_calcParameters.CSVCharacter = ";";
            VLoadType vlt = new VLoadType("vlt", string.Empty, "W", "kWh", 1000, 1, new TimeSpan(1, 0, 0), 1,
                string.Empty, LoadTypePriority.Mandatory,true,Guid.NewGuid().ToString(), 1);

            ObservableCollection<VLoadType> vlts = new ObservableCollection<VLoadType> {vlt};
            var dtoDict =  CalcLoadTypeDtoFactory.MakeLoadTypes(vlts, new TimeSpan(0, 0, 1),LoadTypePriority.All);
            var ltdict = CalcLoadTypeFactory.MakeLoadTypes(dtoDict);
            var dto = dtoDict.GetLoadtypeDtoByLoadType(vlt);
            var clt = ltdict.GetCalcLoadTypeByLoadtype(dto);
            Assert.AreEqual(clt.ConversionFactor, 1.0/3600000);
        }
    }
}
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

using Automation.ResultFiles;
using Common.CalcDto;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class TotalsPerLoadtypeEntry : IHouseholdKey {
        public TotalsPerLoadtypeEntry([JetBrains.Annotations.NotNull] HouseholdKey key, [JetBrains.Annotations.NotNull] CalcLoadTypeDto loadtype, double value, int numberOfPersons, double totalDays)
        {
            HouseholdKey = key;
            Loadtype = loadtype;
            Value = value;
            NumberOfPersons = numberOfPersons;
            TotalDays = totalDays;
        }
        /*

        s += dstLoadType.UnitOfPower + "*" + _calcParameters.InternalStepsize.TotalSeconds +
        " seconds" + _calcParameters.CSVCharacter;
        s += sum* dstLoadType.ConversionFactor + _calcParameters.CSVCharacter +
        dstLoadType.UnitOfSum;
        var totaldays =
            (_calcParameters.OfficialEndTime - _calcParameters.OfficialStartTime)
            .TotalDays;
        s += _calcParameters.CSVCharacter + sum* dstLoadType.ConversionFactor / totaldays +

        _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
        s += _calcParameters.CSVCharacter + min + _calcParameters.CSVCharacter +
        dstLoadType.UnitOfPower;
        s += _calcParameters.CSVCharacter + max + _calcParameters.CSVCharacter +
        dstLoadType.UnitOfPower;
        s += _calcParameters.CSVCharacter + sum* dstLoadType.ConversionFactor / numberOfPersons +
        _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;
        s += _calcParameters.CSVCharacter +
        sum* dstLoadType.ConversionFactor / numberOfPersons / totaldays +
        _calcParameters.CSVCharacter + dstLoadType.UnitOfSum;

        _calcParameters.CSVCharacter + "Units" +
        _calcParameters.CSVCharacter + "Readable" +
        _calcParameters.CSVCharacter + "Units" +
        _calcParameters.CSVCharacter + "Per Day" +
        _calcParameters.CSVCharacter + "Units" +
        _calcParameters.CSVCharacter + "Minimum Values" +
        _calcParameters.CSVCharacter + "Minmum Value Unit" +
        _calcParameters.CSVCharacter + "Maximum Values" +
        _calcParameters.CSVCharacter + "Maximum Value Unit" +
        _calcParameters.CSVCharacter + "Per Person" +
        _calcParameters.CSVCharacter + "Unit" +
        _calcParameters.CSVCharacter + "Per Person and Day" +
        _calcParameters.CSVCharacter + "Unit");*/
        [JetBrains.Annotations.NotNull]
        [JsonProperty]
        public CalcLoadTypeDto Loadtype { get; private set; }

        [JsonProperty]
        public double Value { get; private set; }

        public int NumberOfPersons { get; }
        public double TotalDays { get; }

        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private set; }
    }
}
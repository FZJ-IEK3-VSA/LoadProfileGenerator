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
using Common.SQLResultLogging.Loggers;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.JSON {
    public class LocationEntry :IHouseholdKey {
        [JsonProperty]
        public HouseholdKey HouseholdKey { get; private set; }

        [NotNull]
        [JsonProperty]
        public string LocationName { get; private set; }

        [NotNull]
        [JsonProperty]
        public string LocationGuid { get; private set; }

        [NotNull]
        [JsonProperty]
        public  string PersonName { get; private set; }
        [NotNull]
        [JsonProperty]
        public string PersonGuid { get; private set; }
        [JsonProperty]
        [NotNull]
        public TimeStep Timestep { get; private set; }

        public LocationEntry([NotNull] HouseholdKey householdKey,
                             [NotNull] string personName, [NotNull] string personGuid,
                             [NotNull] TimeStep pTimestep,
                             [NotNull] string locationName, [NotNull] string locationGuid
                             ) {
            HouseholdKey = householdKey;
            PersonName = personName;
            PersonGuid = personGuid;
            Timestep = pTimestep;
            LocationName = locationName;
            LocationGuid = locationGuid;
        }

        /*public override string ToString()
        {
            DateStampCreator dsc = new DateStampCreator(_calcParameters);
            return _timestep + _calcParameters.CSVCharacter +
                   dsc.MakeDateStringFromTimeStep(_timestep) +
                   _calcParameters.CSVCharacter +
                   _person.Name + _calcParameters.CSVCharacter +
                   _location.Name;
        }*/
    }
}
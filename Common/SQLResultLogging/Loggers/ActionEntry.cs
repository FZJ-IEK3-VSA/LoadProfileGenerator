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
using System.Diagnostics.CodeAnalysis;
using Automation.ResultFiles;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Common.SQLResultLogging.Loggers {
    public class SingleTimestepActionEntry : IHouseholdKey
    {
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [UsedImplicitly]
        [Obsolete("Only for json")]
        public SingleTimestepActionEntry()
        {
            //for npoco deserializer
        }

        public SingleTimestepActionEntry([NotNull] HouseholdKey householdKey, int timeStep,
                           DateTime dateTime, [NotNull] string personGuid, [CanBeNull] string actionEntryGuid)
        {
            HouseholdKey = householdKey;
            TimeStep = timeStep;
            DateTime = dateTime;
            PersonGuid = personGuid;
            if (actionEntryGuid == null) {
                throw new LPGException("ActionEntryGuid was null");
            }
            ActionEntryGuid = actionEntryGuid;
        }

        [JsonProperty]
        public DateTime DateTime { get; set; }

        [NotNull]
        [JsonProperty]
        public string PersonGuid { get; set; }

        [NotNull]
        [JsonProperty]
        public string ActionEntryGuid { get; set; }

        [JsonProperty]
        public int TimeStep { get; set; }

        [JsonProperty]
        public HouseholdKey HouseholdKey { get; set; }
    }
    public class ActionEntry : IHouseholdKey {
        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        [UsedImplicitly]
        public ActionEntry()
        {
            //for npoco deserializer
        }

        public ActionEntry([NotNull] string category, [NotNull] HouseholdKey householdKey, [NotNull] TimeStep timeStep,
                           DateTime dateTime, [NotNull] string personGuid, [NotNull] string personName, bool isSick,
                           [NotNull] string affordanceName, [NotNull] string affordanceGuid, int id)
        {
            Category = category;
            HouseholdKey = householdKey;
            TimeStep = timeStep;
            DateTime = dateTime;
            PersonGuid = personGuid;
            PersonName = personName;
            IsSick = isSick;
            AffordanceName = affordanceName;
            AffordanceGuid = affordanceGuid;
            ID = id;
            ActionEntryGuid = Guid.NewGuid().ToString();
        }

        [JsonProperty]
        [NotNull]
        public string ActionEntryGuid { get; set; }

        [NotNull]
        [JsonProperty]
        public string AffordanceGuid { get; set; }

        [NotNull]
        [JsonProperty]
        public string AffordanceName { get; set; }

        [NotNull]
        [JsonProperty]
        public string Category { get; set; }

        [JsonProperty]
        public DateTime DateTime { get; set; }

        [JsonProperty]
        public int ID { get; set; }

        [JsonProperty]
        public bool IsSick { get; set; }

        [NotNull]
        [JsonProperty]
        public string PersonGuid { get; set; }

        [NotNull]
        [JsonProperty]
        public string PersonName { get; set; }

        [JsonProperty]
        [NotNull]
        public TimeStep TimeStep { get; set; }

        [JsonProperty]
        public HouseholdKey HouseholdKey { get; set; }

        [NotNull]
        public static ActionEntry MakeActionEntry([NotNull] TimeStep timeStep, [NotNull] string personGuid,
                                                  [NotNull] string personName,
                                                  bool isSick, [NotNull] string affordanceName,
                                                  [NotNull] string affordanceGuid, [NotNull] HouseholdKey householdKey,
                                                  [NotNull] string category,
                                                  DateTime timestamp)
        {
            ActionEntry ae = new ActionEntry(
                category, householdKey, timeStep, timestamp, personGuid, personName, isSick, affordanceName,
                affordanceGuid,
                0);
            return ae;
        }
    }
}
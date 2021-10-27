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
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; L
// OSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
// ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

// </copyright>

//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Automation;
using Common.Enums;
using JetBrains.Annotations;

namespace Common.CalcDto {
    public class CalcAffordanceTaggingSetDto {
        [NotNull] private readonly Dictionary<string, string> _affordanceToTagDict;

        [ItemNotNull] [NotNull] private readonly List<CalcLoadTypeDto> _loadTypes = new List<CalcLoadTypeDto>();

        [ItemNotNull] [NotNull] private readonly List<ReferenceEntry> _referenceEntries = new List<ReferenceEntry>();

        public CalcAffordanceTaggingSetDto([NotNull] string name, bool makeCharts)
        {
            Name = name;
            MakeCharts = makeCharts;
            _affordanceToTagDict = new Dictionary<string, string>();
        }

        // this property is needed for Json deserialization
        [NotNull]
        public Dictionary<string, string> AffordanceToTagDict => _affordanceToTagDict;

        [NotNull]
        public Dictionary<string, ColorRGB> Colors { get; } = new Dictionary<string, ColorRGB>();

        public bool HasReferenceEntries => _referenceEntries.Count > 0;

        [ItemNotNull]
        [NotNull]
        public List<CalcLoadTypeDto> LoadTypes => _loadTypes;

        public bool MakeCharts { get; }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public string IdlenessTag { get; set; } = "idleness";

        /// <summary>
        /// Gets the affordance tag assigned to the specified affordance name. Checks for special affordance names.
        /// </summary>
        /// <param name="affordanceName">The name of the affordance for which the tag is returned</param>
        /// <returns>The tag of the specified affordance</returns>
        public string GetAffordanceTag(string affordanceName)
        {
            // look up the affordance name in the dictionary
            if (!_affordanceToTagDict.TryGetValue(affordanceName, out var affordanceTag))
            {
                // the affordance name was not found - check for special affordance names
                if (affordanceName.StartsWith("Idleness"))
                {
                    // a separate idleness affordance is generated for each person for each location
                    return IdlenessTag;
                }
                else
                {
                    throw new Automation.ResultFiles.LPGException("The affordance \"" + affordanceName + "\" is not contained in tagging set \"" + Name + "\"");
                }
            }
            return affordanceTag;
        }

        /// <summary>
        /// Returns whether the affordance name is contained in this tagging set
        /// </summary>
        /// <param name="affordanceName">The name of the affordance</param>
        /// <returns>true if the affordance is contained, else false</returns>
        public bool ContainsAffordance(string affordanceName)
        {
            return _affordanceToTagDict.ContainsKey(affordanceName);
        }

        /// <summary>
        /// Returns an Enumerable to iterate through all affordance names and assigned tags
        /// </summary>
        /// <returns>An Enumerable of KeyValuePairs, each containing one affordance name and the assigned tag</returns>
        public IEnumerable<KeyValuePair<string, string>> GetAffordanceTagEntries()
        {
            return _affordanceToTagDict.AsEnumerable<KeyValuePair<string, string>>();
        }

        public void AddLoadType([NotNull] CalcLoadTypeDto calcLoadType)
        {
            _loadTypes.Add(calcLoadType);
        }

        public void AddReference([NotNull] string tagName, PermittedGender gender, int minage, int maxAge,
                                 double referenceValue)
        {
            var re = new ReferenceEntry(tagName, gender, minage, maxAge, referenceValue);
            _referenceEntries.Add(re);
        }

        public void AddTag([NotNull] string affordanceName, [NotNull] string tagName)
        {
            _affordanceToTagDict.Add(affordanceName, tagName);
        }

        public double LookupReferenceValue([NotNull] string tagName, PermittedGender gender, int age)
        {
            var re =
                _referenceEntries.FirstOrDefault(
                    x => x.Tag == tagName && x.Gender == gender && age >= x.MinAge && age <= x.MaxAge);
            if (re != null) {
                return re.ReferenceValue;
            }

            return 0;
        }

        [ItemNotNull]
        [NotNull]
        public List<string> MakeReferenceTags()
        {
            return _referenceEntries.Select(x => x.Tag).ToList();
        }

        private class ReferenceEntry {
            public ReferenceEntry([NotNull] string tag, PermittedGender gender, int minAge, int maxAge,
                                  double referenceValue)
            {
                Tag = tag;
                Gender = gender;
                MinAge = minAge;
                MaxAge = maxAge;
                ReferenceValue = referenceValue;
            }

            public PermittedGender Gender { get; }
            public int MaxAge { get; }
            public int MinAge { get; }
            public double ReferenceValue { get; }

            [NotNull]
            public string Tag { get; }
        }
    }
}
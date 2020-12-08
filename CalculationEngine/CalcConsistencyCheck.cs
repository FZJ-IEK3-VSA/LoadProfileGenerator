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

using CalculationEngine.HouseholdElements;
using Common;
using Common.JSON;

namespace CalculationEngine
{
    internal static class CalcConsistencyCheck
    {
        public static void CheckConsistency([JetBrains.Annotations.NotNull] CalcHousehold chh, [JetBrains.Annotations.NotNull] CalcParameters calcParameters)
        {
            CheckTimeResolution(calcParameters);
            // check for the same device twice
            foreach (CalcLocation location in chh.Locations)
            {
                foreach (var calcAffordanceBase in location.Affordances) {
                    ICalcAffordanceBase affordance = calcAffordanceBase;
                    if (affordance.AreThereDuplicateEnergyProfiles())
                    {
                        throw new DataIntegrityException(
                            "Same device twice in one Affordance: " + location.Name + " - " + affordance.Name);
                    }
                }
            }

            // check for timeprofiles without values
            foreach (CalcLocation calcLocation in chh.Locations)
            {
                foreach (var calcAffordanceBase in calcLocation.Affordances) {
                    ICalcAffordanceBase calcAffordance = calcAffordanceBase;
                    if (calcAffordance.AreDeviceProfilesEmpty() != null)
                    {
                        throw new DataIntegrityException("Timeprofile without values: " +
                                                         calcAffordance.AreDeviceProfilesEmpty());
                    }
                }
            }

            // persons  without desires
            foreach (CalcPerson calcPerson in chh.Persons)
            {
                if (calcPerson.DesireCount == 0)
                {
                    throw new DataIntegrityException("Person without desires: " + calcPerson.Name);
                }
            }

            if (calcParameters.InternalStepsize.TotalSeconds < 1)
            {
                throw new DataIntegrityException("Time resolution too small!");
            }
        }

        private static void CheckTimeResolution([JetBrains.Annotations.NotNull] CalcParameters calcParameters)
        {
            int internalseconds = (int)calcParameters.InternalStepsize.TotalSeconds;
            int externalseconds = (int)calcParameters.ExternalStepsize.TotalSeconds;
            if (externalseconds%internalseconds != 0)
            {
                throw new DataIntegrityException(
                    "The external time resolution needs to be an even multiple of the internal time resolution.");
            }
        }
    }
}

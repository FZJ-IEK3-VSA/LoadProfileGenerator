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

#region

using System;
using System.Diagnostics.CodeAnalysis;

#endregion

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// A single profile for a single device which occurs in an affordance.
    /// </summary>
    public class DeviceEnergyProfileTuple
    {
        [JetBrains.Annotations.NotNull] private readonly CalcDevice _calcDevice;

        private readonly double _multiplier;

        public DeviceEnergyProfileTuple([JetBrains.Annotations.NotNull] CalcDevice pdev, [JetBrains.Annotations.NotNull] CalcProfile ep,
                                        [JetBrains.Annotations.NotNull] CalcLoadType pLoadType, decimal timeOffset, TimeSpan stepsize,
                                        double multiplier, double probability)
        {
            _calcDevice = pdev;
            TimeProfile = ep;
            LoadType = pLoadType;
            TimeOffset = timeOffset;
            _multiplier = multiplier;
            var minutesperstep = (decimal)stepsize.TotalMinutes;
            TimeOffsetInSteps = (int)(timeOffset / minutesperstep);
            Probability = probability;
        }

        [JetBrains.Annotations.NotNull]
        public CalcDevice CalcDevice => _calcDevice;

        [JetBrains.Annotations.NotNull]
        public CalcLoadType LoadType { get; }

        public double Multiplier => _multiplier;

        public double Probability { get; }

        public decimal TimeOffset { get; }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "InSteps")]
        public int TimeOffsetInSteps { get; }

        [JetBrains.Annotations.NotNull]
        public CalcProfile TimeProfile { get; }

        [JetBrains.Annotations.NotNull]
        public override string ToString() => "Device:" + _calcDevice.Name + ", Profile " + TimeProfile.Name + ", Offset " + TimeOffset;
    }
}

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
using JetBrains.Annotations;

#endregion

namespace Common
{
    /// <summary>
    ///     Normal (aka Gaussian) distribution; See the
    ///     <A HREF="http:// www.cern.ch/RD11/rkb/AN16pp/node188.html#SECTION0001880000000000000000"> math definition</A>
    ///     and
    ///     <A HREF="http:// www.statsoft.com/textbook/glosn.html#Normal Distribution">animated definition    </A>
    ///     .
    ///     <pre>
    ///         1 2
    ///         pdf(x) = --------- exp( - (x-mean) / 2v )
    ///         sqrt(2pi*v)
    ///         x
    ///         -
    ///         1 | | 2
    ///         cdf(x) = --------- | exp( - (t-mean) / 2v ) dt
    ///         sqrt(2pi*v)| |
    ///         -
    ///         -inf.
    ///     </pre>
    ///     where <tt>v = variance = standardDeviation^2</tt> .
    ///     <p /> Instance methods operate on a user supplied uniform random number generator; they are unsynchronized. <dt />
    ///     <p />
    ///     <b>Implementation:</b>
    ///     Polar Box-Muller transformation. See
    ///     G.E.P. Box, M.E. Muller (1958): A note on the generation of random normal deviates, Annals Math. Statist. 29,
    ///     610-611.
    ///     <p />
    /// </summary>
    /// <author>wolfgang.hoschek@cern.ch</author>
    /// <version>1.0, 09/24/99</version>
    public class NormalRandom
    {
        [NotNull]
        private readonly Random _randomGenerator;
        private double _cache; // cache for Box-Mueller algorithm
        private bool _cacheFilled; // Box-Mueller
        private double _mean;
        private double _standardDeviation;

        /// <summary>
        ///  Constructs a normal (gauss) distribution. Example: mean=0.0, standardDeviation=1.0.
        /// </summary>
        /// <param name="mean">mean </param>
        /// <param name="standardDeviation">standard deviation</param>
        /// <param name="randomgenerator">rng</param>
        public NormalRandom(double mean, double standardDeviation, [NotNull] Random randomgenerator)
        {
            _randomGenerator = randomgenerator;
            SetState(mean, standardDeviation);
        }

        /// <summary>
        /// ///     Returns a String representation of the receiver.
        /// </summary>
        /// <returns>the string</returns>
        [NotNull]
        public override string ToString() => GetType().FullName + "(" + _mean + "," + _standardDeviation + ")";

        /// <summary>
        ///     Returns a random number from the distribution; bypasses the internal state.
        /// </summary>
        /// <param name="mean">todo: describe mean parameter on NextDouble</param>
        /// <param name="standardDeviation">todo: describe standardDeviation parameter on NextDouble</param>
        /// <returns>the next double</returns>
        public double NextDouble(double mean, double standardDeviation)
        {
            // Uses polar Box-Muller transformation.
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (_cacheFilled && (_mean == mean) &&
                // ReSharper restore CompareOfFloatsByEqualityOperator
                // ReSharper disable CompareOfFloatsByEqualityOperator
                (_standardDeviation == standardDeviation))
                // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                _cacheFilled = false;
                return _cache;
            }

            double x;
            double y;
            double r;
            do
            {
                x = 2.0*_randomGenerator.NextDouble() - 1.0;
                y = 2.0*_randomGenerator.NextDouble() - 1.0;
                r = x*x + y*y;
            } while (r >= 1.0);

            double z = Math.Sqrt(-2.0*Math.Log(r)/r);
            _cache = mean + standardDeviation*x*z;
            _cacheFilled = true;
            return mean + standardDeviation*y*z;
        }

        /// <summary>
        ///     Sets the mean and variance.
        /// </summary>
        /// <param name="mean">todo: describe mean parameter on SetState</param>
        /// <param name="standardDeviation">todo: describe standardDeviation parameter on SetState</param>
        private void SetState(double mean, double standardDeviation)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if ((mean != _mean) || (standardDeviation != _standardDeviation))
                // ReSharper restore CompareOfFloatsByEqualityOperator
            {
                _mean = mean;
                _standardDeviation = standardDeviation;
                _cacheFilled = false;
            }
        }
    }
}
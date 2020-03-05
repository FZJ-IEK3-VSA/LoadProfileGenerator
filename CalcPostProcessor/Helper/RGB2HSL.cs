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
using Automation;
using Common;

namespace CalcPostProcessor.Helper {

    /// <summary>
    ///     Utility struct for color conversions
    /// </summary>
    internal static class RGBHSL {
        [SuppressMessage("ReSharper", "SwitchStatementMissingSomeCases")]
        public static ColorRGB HSL2RGB(double myH, double sl, double l)
        {
            var h = myH;
            var r = l;
            var g = l;
            var b = l;
            var v = l <= 0.5 ? l * (1.0 + sl) : l + sl - l * sl;
            if (v > 0) {
                var m = l + l - v;
                var sv = (v - m) / v;
                h *= 6.0;
                var sextant = (int) h;
                var fract = h - sextant;
                var vsf = v * sv * fract;
                var mid1 = m + vsf;
                var mid2 = v - vsf;
                switch (sextant) {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            var rgb = new ColorRGB {
                R = Convert.ToByte(r * 255.0f),
                G = Convert.ToByte(g * 255.0f),
                B = Convert.ToByte(b * 255.0f)
            };
            return rgb;
        }

        // Given a Color (RGB Struct) in range of 0-255

        // Return H,S,L in range of 0-1

        /// <summary>
        ///     RGB to HSL conversion
        /// </summary>
        /// <param name="rgb"> The RGB. </param>
        /// <param name="h"> The h. </param>
        /// <param name="s"> The s. </param>
        /// <param name="l"> The l. </param>
        public static void RGB2HSL(ColorRGB rgb, out double h, out double s, out double l)
        {
            var r = rgb.R / 255.0;
            var g = rgb.G / 255.0;
            var b = rgb.B / 255.0;

            h = 0; // default to black
            s = 0;
            var v = Math.Max(r, g);
            v = Math.Max(v, b);
            var m = Math.Min(r, g);
            m = Math.Min(m, b);

            l = (m + v) / 2.0;
            if (l <= 0.0) {
                return;
            }
            var vm = v - m;
            s = vm;
            if (s > 0.0) {
                s /= l <= 0.5 ? v + m : 2.0 - v - m;
            }
            else {
                return;
            }
            var r2 = (v - r) / vm;
            var g2 = (v - g) / vm;
            var b2 = (v - b) / vm;
            if (Math.Abs(r - v) < Constants.Ebsilon) {
                h = Math.Abs(g - m) < Constants.Ebsilon ? 5.0 + b2 : 1.0 - g2;
            }
            else if (Math.Abs(g - v) < Constants.Ebsilon) {
                h = Math.Abs(b - m) < Constants.Ebsilon ? 1.0 + r2 : 3.0 - b2;
            }
            else {
                h = Math.Abs(r - m) < Constants.Ebsilon ? 3.0 + g2 : 5.0 - r2;
            }
            h /= 6.0;
        }
    }
}
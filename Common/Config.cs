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
using System.Globalization;
using Automation.ResultFiles;
using JetBrains.Annotations;

#endregion

namespace Common {
    public sealed class Config {
        [CanBeNull] private static Config _config;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Config()
        {
            DummyTime = new DateTime(1900, 1, 1);
        }

        private Config([NotNull] string startpath) => StartPath = startpath;

        [NotNull]
        public static CultureInfo CultureInfo { get; } = CultureInfo.CurrentCulture;

        public static DateTime DummyTime { get; }

        public static bool ExtraUnitTestChecking { get; set; }

        //public static bool ResetLogfileAtCalculationStart { get; set; }

        public static bool IsInUnitTesting { get; set; }
        public static bool IsInHeadless { get; set; }
        public static bool MakePDFCharts { get; set; }
        public static bool ReallyMakeAllFilesIncludingBothSums { get; set; }
        public static bool ShowDeleteMessages { get; set; } = true;
        [CanBeNull]
        public static int? SpecialChartFontSize { get; set; }

#pragma warning disable VSD0048 // A property with a private setter can become a read-only property instead.
        [CanBeNull]
        public static string StartPath { get; private set; }
#pragma warning restore VSD0048 // A property with a private setter can become a read-only property instead.

        [CanBeNull]
        public static string WarningString { get; set; }
        public static bool AdjustTimesForSettlement { get; set; } = true;
        public static bool SkipFreeSpaceCheckForCalculation { get; set; }
        public static bool AllowEmptyHouses { get; set; }
        public static bool CatchErrors { get; set; }
        public static bool OutputToConsole { get; set; }

        public static void InitConfig([NotNull] string startpath) {
            if (_config != null) {
                throw new LPGException("Config was already initialized");
            }
            _config = new Config(startpath);
        }
    }
}
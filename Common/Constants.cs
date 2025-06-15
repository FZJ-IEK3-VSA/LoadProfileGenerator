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

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using Automation;
using Automation.ResultFiles;

namespace Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreAttribute : Attribute
    {
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ColorConstants
    {
        public static ColorRGB White { get; } = new ColorRGB(255, 255, 255);
        public static ColorRGB Red { get; } = new ColorRGB(255, 0, 0);
        public static ColorRGB Orange { get; } = new ColorRGB(255, 165, 0);
        public static ColorRGB DarkOrange { get; } = new ColorRGB(255, 140, 0);
        public static ColorRGB DeepSkyBlue { get; } = new ColorRGB(0, 191, 255);
        public static ColorRGB AntiqueWhite { get; } = new ColorRGB(250, 235, 215);
        public static ColorRGB Black { get; } = new ColorRGB(0, 0, 0);

    }

    public static class Constants
    {
        public const string SettlementJsonName = "SettlementInformation.json";
        public const double Ebsilon = 0.000000001;
        public const int PermittedGenderNumber = 3;
        public const string TakingAVacationString = "taking a vacation";

        /// <summary>
        /// Specifies how many times opening a DB file will be attempted before aborting,
        /// if the database is locked or otherwise temporarily unavailable.
        /// </summary>
        public const int MaxDbOpenAttempts = 5;

        /// <summary>
        /// The name of the site referring to the home of the simulated persons.
        /// </summary>
        public const string HomeSiteName = "Home";
        public const string FinishedFileFlag = "finished.flag";

        public static HouseholdKey GeneralHouseholdKey { get; } = new HouseholdKey("General");

        public static HouseholdKey HouseKey { get; } = new HouseholdKey("House");

        public static StrGuid HouseLocationGuid { get; } = StrGuid.FromString("5A8A1AC5-0EAC-462B-BB14-3ED3A789EBC2");
        public const string UnknownTag = "Unknown Tag";

        public const string TableDescriptionTableName = "TableDescription";

        public const string CalculationProfilerJson = "CalculationProfiler.json";

        /// <summary>
        /// Name of the directory containing all databases in a city simulation."
        /// </summary>
        public const string DataBaseDirectory = "Databases";

        /// <summary>
        /// Name of the directory containing the result directories for all houses.
        /// </summary>
        public const string HousesDirectory = "Houses";

        /// <summary>
        /// Name of the directory containing additional log files from the city simulation.
        /// </summary>
        public const string CityLogDirectory = "Logs";

        /// <summary>
        /// Name of the file containing the random seed used for each house in a city simulation.
        /// </summary>
        public const string HouseSeedMappingFile = "HouseRNGSeeds.json";
    }

    public enum TargetDirectory
    {
        Undefined,
        Root,
        Results,
        Reports,
        Charts,
        Debugging,
        Temporary
    }
}
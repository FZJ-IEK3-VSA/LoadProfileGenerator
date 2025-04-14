using System.Collections.Generic;
using System;
using Database.Tables.ModularHouseholds;
using Database.Tables.BasicElements;
using Common;
using Automation.ResultFiles;
using Common.JSON;
using Common.CalcDto;

namespace CalculationController.DtoFactories
{
    /// <summary>
    /// Manages creation of availability reference objects from timelimits for a single household.
    /// </summary>
    /// <param name="availabilityDtoRepository">the global repository to store all availability references</param>
    /// <param name="calcParams">the calculation parameters</param>
    /// <param name="rnd">the random object to use</param>
    /// <param name="temperatureProfile">temperature profile for the time limits</param>
    /// <param name="geographicLocation">geographic location for the time limits</param>
    /// <param name="vacationTimeframes">vacation times for the time limits</param>
    /// <param name="holidayKey">holiday key for the time limits</param>
    public class AvailabilityFactory(AvailabilityDtoRepository availabilityDtoRepository, CalcParameters calcParams, Random rnd,
        TemperatureProfile temperatureProfile, GeographicLocation geographicLocation, List<VacationTimeframe> vacationTimeframes, string holidayKey)
    {
        /// <summary>
        /// Collects the bridge days from all timelimits
        /// </summary>
        public HashSet<DateTime> BridgeDays { get; } = [];

        /// <summary>
        /// Creates an availability reference for an affordance with timelimit from a household trait.
        /// </summary>
        /// <param name="affordance">the affordance with timelimit</param>
        /// <returns>the new availability reference</returns>
        /// <exception cref="DataIntegrityException">if the affordance itself has no timelimit</exception>
        /// <exception cref="LPGException">if the root entry of the timelimit was null</exception>
        public AvailabilityDataReferenceDto CreateAvailabilityForTimeLimitAff(AffordanceWithTimeLimit affordance)
        {
            // select the TimeLimit to use
            if (affordance.Affordance.TimeLimit == null)
                throw new DataIntegrityException("The time limit on the affordance was null. Please fix", affordance.Affordance);

            var tl = affordance.Affordance.TimeLimit;
            if (affordance.TimeLimit != null)
            {
                tl = affordance.TimeLimit;
            }
            if (tl.RootEntry == null)
            {
                throw new LPGException("Root Entry was null");
            }
            return CreateAvailabilityArray(tl, true, affordance.StartMinusTime, affordance.StartPlusTime, affordance.EndMinusTime, affordance.EndPlusTime);
        }

        /// <summary>
        /// Creates an availability reference from a timelimit.
        /// </summary>
        /// <param name="timeLimit">the timelimit to use</param>
        /// <returns>the new availability reference</returns>
        public AvailabilityDataReferenceDto CreateAvailabilityFromTimeLimit(TimeLimit timeLimit)
        {
            return CreateAvailabilityArray(timeLimit);
        }

        /// <summary>
        /// Creates the bitarray of the timelimit and turns it into an availability reference, with optional adjustments.
        /// </summary>
        /// <param name="tl">the timelimit to use</param>
        /// <param name="invert">whether the bitarray should be inverted (swap true and false)</param>
        /// <param name="startMinus">minus variation of the start time</param>
        /// <param name="startPlus">plus variation of the start time</param>
        /// <param name="endMinus">minus variation of the end time</param>
        /// <param name="endPlus">plus variation of the end time</param>
        /// <returns>the new availability reference></returns>
        private AvailabilityDataReferenceDto CreateAvailabilityArray(TimeLimit tl, bool invert = false, int startMinus = 0, int startPlus = 0, int endMinus = 0, int endPlus = 0)
        {
            // create the bitarray for the timelimit
            var bitarray = tl.RootEntry.GetOneYearArray(calcParams.InternalStepsize, calcParams.InternalStartTime,
                calcParams.InternalEndTime, temperatureProfile, geographicLocation, rnd, vacationTimeframes, holidayKey,
                out var newBridgeDays, startMinus, startPlus, endMinus, endPlus);
            BridgeDays.UnionWith(newBridgeDays);

            if (invert)
                bitarray = bitarray.Not();
            return availabilityDtoRepository.MakeNewReference(tl.Name, bitarray);
        }
    }
}
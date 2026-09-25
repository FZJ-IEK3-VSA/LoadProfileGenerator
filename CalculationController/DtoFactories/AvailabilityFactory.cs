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
    /// Specifies additional parameters to use for creating the bitarry for an availability reference.
    /// Unlike the parameters specified in the availability factory constructor, these parameters can
    /// vary within a household.
    /// </summary>
    /// <param name="TimeLimitName">the name of the used timelimit</param>
    /// <param name="Invert">whether the bitarray should be inverted (swap true and false)</param>
    /// <param name="StartMinus">minus variation of the start time</param>
    /// <param name="StartPlus">plus variation of the start time</param>
    /// <param name="EndMinus">minus variation of the end time</param>
    /// <param name="EndPlus">plus variation of the end time</param>
    internal record AvailabilityParams(string TimeLimitName, bool Invert = false, int StartMinus = 0, int StartPlus = 0, int EndMinus = 0, int EndPlus = 0);

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
        /// Stores all availability references created by this factory. As the basic parameters are always the same,
        /// availabilities for a specific timelimit can be reused, if the AvailabilityParams are the same.
        /// </summary>
        private Dictionary<AvailabilityParams, AvailabilityDataReferenceDto> CreatedReferences { get; } = [];

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
            var parameters = new AvailabilityParams(tl.Name, true, affordance.StartMinusTime, affordance.StartPlusTime, affordance.EndMinusTime, affordance.EndPlusTime);
            return CreateAvailabilityArray(tl, parameters);
        }

        /// <summary>
        /// Creates an availability reference from a timelimit.
        /// </summary>
        /// <param name="timeLimit">the timelimit to use</param>
        /// <param name="invert">if true, the timelimit is inverted, so all bit values are changed</param>
        /// <returns>the new availability reference</returns>
        public AvailabilityDataReferenceDto CreateAvailabilityFromTimeLimit(TimeLimit timeLimit, bool invert = false)
        {
            var parameters = new AvailabilityParams(timeLimit.Name, invert);
            return CreateAvailabilityArray(timeLimit, parameters);
        }

        /// <summary>
        /// Creates the bitarray of the timelimit and turns it into an availability reference, with optional adjustments.
        /// </summary>
        /// <param name="tl">the timelimit to use</param>
        /// <param name="parameters">additional parameter influencing the final bitarray</param>
        /// <returns>the new availability reference></returns>
        private AvailabilityDataReferenceDto CreateAvailabilityArray(TimeLimit tl, AvailabilityParams parameters)
        {
            if (CreatedReferences.TryGetValue(parameters, out var reference))
            {
                // the bitarray and availability reference for these parameters was already created by this factory and can be reused
                return reference;
            }

            // create the bitarray for the timelimit
            var bitarray = tl.RootEntry.GetOneYearArray(calcParams.InternalStepsize, calcParams.InternalStartTime,
                calcParams.InternalEndTime, temperatureProfile, geographicLocation, rnd, vacationTimeframes, holidayKey,
                out var newBridgeDays, parameters.StartMinus, parameters.StartPlus, parameters.EndMinus, parameters.EndPlus);
            BridgeDays.UnionWith(newBridgeDays);

            if (parameters.Invert)
                bitarray = bitarray.Not();

            // create the new reference and cache it so it can be reused
            var newReference = availabilityDtoRepository.MakeNewReference(tl.Name, bitarray);
            CreatedReferences[parameters] = newReference;
            return newReference;
        }
    }
}
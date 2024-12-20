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

using Automation;
using Automation.ResultFiles;
using CalculationEngine.ReinforcementLearning;
using Common;
using Common.SQLResultLogging;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

#endregion

namespace CalculationEngine.HouseholdElements
{
    public class CalcPersonDesires {
        private readonly CalcRepo _calcRepo;

        [NotNull]
        private static readonly Dictionary<Tuple<string, HouseholdKey>, int> _persons =
            new Dictionary<Tuple<string, HouseholdKey>, int>();
        [ItemNotNull]
        [NotNull]
        private List<string> _lastAffordances = new List<string>();
        [ItemNotNull]
        [NotNull]

        private readonly DateStampCreator _dsc;
        private StreamWriter? _sw;

        public bool _debug_print = false;

        public CalcPersonDesires(CalcRepo calcRepo) {
            _calcRepo = calcRepo;
            Desires = new Dictionary<int, CalcDesire>();
            _persons.Clear();
            _dsc = new DateStampCreator(_calcRepo.CalcParameters);
        }

        [NotNull]
        public Dictionary<int, CalcDesire> Desires { get; }

        public void AddDesires([NotNull] CalcDesire cd) {
            if (!Desires.ContainsKey(cd.DesireID)) {
                Desires.Add(cd.DesireID, cd);
            }
        }


        /// <summary>
        /// Applies the linear effect of an affordance to the person's desires over a specified duration.
        /// </summary>
        /// <param name="satisfactionvalues">
        /// A list of <see cref="CalcDesire"/> objects representing the desires affected by the affordance.
        /// </param>
        /// <param name="randomEffect">
        /// A flag indicating whether random effects are applied (not used in the current implementation).
        /// </param>
        /// <param name="affordance">
        /// The name of the affordance being applied.
        /// </param>
        /// <param name="durationInTimeSteps">
        /// The duration over which the affordance's effect is applied, measured in minutes.
        /// </param>
        /// <param name="firsttime">
        /// A flag indicating whether this is the first time the affordance is being applied.
        /// </param>
        /// <remarks>
        /// This method updates the desire values linearly over the specified duration. It also keeps track of the
        /// last applied affordances and their corresponding timestamps for historical record-keeping.
        /// </remarks>
        public void ApplyAffordanceEffect_Linear([NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, bool randomEffect,
            [NotNull] string affordance, int durationInTimeSteps, Boolean firsttime) {
            if (firsttime)
            {
                // Maintain a history of the last 10 affordances by removing the oldest entries
                while (_lastAffordances.Count > 10)
                {
                    _lastAffordances.RemoveAt(0);
                }

                // Record the affordance name and trim it to the first three words as a key
                _lastAffordances.Add(affordance);

            }

            // Apply the satisfaction value of the affordance linearly over the duration to each desire
            foreach (var satisfactionvalue in satisfactionvalues)
            {
                // Check if the desire exists in the person's desire dictionary
                if (Desires.ContainsKey(satisfactionvalue.DesireID))
                {
                    // Increment the desire's value linearly by distributing the satisfaction value over the duration
                    Desires[satisfactionvalue.DesireID].Value += satisfactionvalue.Value / durationInTimeSteps;

                    // Ensure that the desire value does not exceed the maximum threshold of 1
                    if (Desires[satisfactionvalue.DesireID].Value > 1)
                    {
                        Desires[satisfactionvalue.DesireID].Value = 1;
                    }
                }
            }
            if (firsttime)
            {
                ApplyRandomEffect(satisfactionvalues, randomEffect);
            }            
        }

        public void ApplyAffordanceEffect([NotNull][ItemNotNull] List<CalcDesire> satisfactionvalues, bool randomEffect,
    [NotNull] string affordance)
        {
            while (_lastAffordances.Count > 10)
            {
                _lastAffordances.RemoveAt(0);
            }
            _lastAffordances.Add(affordance);
            foreach (var satisfactionvalue in satisfactionvalues)
            {
                if (Desires.ContainsKey(satisfactionvalue.DesireID))
                {
                    Desires[satisfactionvalue.DesireID].Value += satisfactionvalue.Value;
                    if (Desires[satisfactionvalue.DesireID].Value > 1)
                    {
                        Desires[satisfactionvalue.DesireID].Value = 1;
                    }
                }
            }
            ApplyRandomEffect(satisfactionvalues, randomEffect);
        }

        private void ApplyRandomEffect(List<CalcDesire> satisfactionvalues, bool randomEffect)
        {
            if (randomEffect)
            {
                var usedDesires = new Dictionary<CalcDesire, bool>();
                foreach (var satisfactionvalue in satisfactionvalues)
                {
                    usedDesires.Add(satisfactionvalue, true);
                }
                var desiresArray = new CalcDesire[Desires.Count];
                Desires.Values.CopyTo(desiresArray, 0);
                var affectedCount = _calcRepo.Rnd.Next(Desires.Count - usedDesires.Count + 1);
                for (var i = 0; i < affectedCount; i++)
                {
                    CalcDesire? d = null;
                    var loopcount = 0;
                    while (d == null)
                    {
                        var selectedkey = _calcRepo.Rnd.Next(Desires.Count);
                        d = desiresArray[selectedkey];
                        loopcount++;
                        if (usedDesires.ContainsKey(d))
                        {
                            d = null;
                        }
                        if (loopcount > 500)
                        {
                            throw new LPGException("Random result failed after 500 tries...");
                        }
                    }
                    d.Value += (decimal)_calcRepo.Rnd.NextDouble();
                    if (d.Value > 1)
                    {
                        d.Value = 1;
                    }
                }
            }
        }

        public void ApplyDecay([NotNull] TimeStep timestep) {
            foreach (var calcDesire in Desires.Values) {
                calcDesire.ApplyDecay(timestep);
            }
        }

        /// <summary>
        /// Applies a decay function to all desires except those specified in the provided list.
        /// </summary>
        /// <param name="timestep">The current simulation time step.</param>
        /// <param name="satisfactionvalues">
        /// A list of desires (as <see cref="CalcDesire"/> objects) that should not have decay applied.
        /// </param>
        /// <remarks>
        /// This method identifies the desires to exclude from decay by creating a set of their IDs.
        /// All other desires in the internal <see cref="Desires"/> collection will have the decay function applied.
        /// </remarks>
        
        public void ApplyDecay_WithoutSome_Linear([NotNull] TimeStep timestep, List<CalcDesire> satisfactionvalues)
        {
            // Create a set of DesireIDs from the satisfactionvalues list.
            // These IDs represent desires that will be excluded from decay.
            HashSet<int> satisfactionDesireIDs = new HashSet<int>(satisfactionvalues.Select(sv => sv.DesireID));

            // Iterate over all desires in the Desires collection.
            foreach (var calcDesire in Desires.Values)
            {
                // Check if the current desire's ID is not in the exclusion set.
                if (!satisfactionDesireIDs.Contains(calcDesire.DesireID))
                {
                    // Apply the decay function to the current desire.
                    calcDesire.ApplyDecay(timestep);
                }
            }
        }


        public decimal CalcEffect([NotNull][ItemNotNull] IEnumerable<CalcDesire> satisfactionvalues, out string? thoughtstring,
            [NotNull] string affordanceName) {
            // calc decay
            foreach (var calcDesire in Desires.Values) {
                calcDesire.TempValue = calcDesire.Value;
            }
            decimal modifier = 1;
            if (_lastAffordances.Contains(affordanceName)) {
                var index = _lastAffordances.IndexOf(affordanceName);
                for (var i = index; i >= 0; i--) {
                    modifier *= 0.9m;
                }
            }
            // add value
            foreach (var satisfactionvalue in satisfactionvalues) {
                if (Desires.ContainsKey(satisfactionvalue.DesireID)) {
                    var desire = Desires[satisfactionvalue.DesireID];
                    if (desire.TempValue + satisfactionvalue.Value > 1) {
                        desire.TempValue += satisfactionvalue.Value / modifier;
                    }
                    else {
                        desire.TempValue += satisfactionvalue.Value * modifier;
                    }
                }
            }
            // get results
            return CalcTotalDeviation(out thoughtstring);
        }

        /// <summary>
        /// Calculates the total deviation, weight sum, and the updated desire values over a specified duration, 
        /// with support for partial linear effects and optional parameters.
        /// </summary>
        /// <param name="duration">The duration for which the effect is calculated. Represents the total action time.</param>
        /// <param name="optionalList">
        /// An optional dictionary of additional desire values, where the key is the desire name and the value is a tuple containing:
        /// <list type="bullet">
        /// <item><description>The desire weight as an integer.</description></item>
        /// <item><description>The initial value of the desire as a double.</description></item>
        /// </list>
        /// </param>
        /// <param name="satValue">
        /// An optional dictionary mapping desire IDs to satisfaction values. These values may adjust the total deviation calculation.
        /// </param>
        /// <param name="interruptable">
        /// A flag indicating whether the effect should be interrupted (true) or calculated for the full duration (false).
        /// Default is false.
        /// </param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><see cref="double"/> totalDeviation: The total deviation of the desire values.</description></item>
        /// <item><description><see cref="double"/> WeightSum: The total weight of the desires affected.</description></item>
        /// <item><description>
        /// A dictionary mapping desire names to their updated weights and values after the effect is applied.
        /// </description></item>
        /// <item><description><see cref="int"/> realDuration: The actual duration of the effect applied.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method updates the temporary values of the desires and calculates the effect based on whether the process 
        /// is interruptable. If interruptable, the effect is limited to 1 unit of duration; otherwise, it spans the specified duration.
        /// </remarks>
        public (double totalDeviation, double WeightSum, Dictionary<string, (int, double)> desireName_ValueAfterApply_Dict, int realDuration) CalcEffect_Linear(int duration, out string? thoughtstring, Dictionary<int, double>? satValue = null, bool? interruptable = false)
        {
            // Set the temporary value of each desire to its current value.
            // This ensures a clean starting point for the calculation.
            foreach (var calcDesire in Desires.Values)
            {
                calcDesire.TempValue = calcDesire.Value;
            }

            // Determine whether the process is interruptable.
            if (interruptable == true)
            {
                // If interruptable, limit the effect calculation to a duration of State Time Intervale in Adapted Q-Learning.
                return CalcTotalDeviation_Linear(AdaptedQLearning.stateTimeIntervale, satValue, out thoughtstring);
            }
            else
            {
                // If not interruptable, calculate the effect for the full specified duration.
                return CalcTotalDeviation_Linear(duration, satValue, out thoughtstring);
            }
        }

        /// <summary>
        /// Retrieves the current values of desires in a linear format, including their weights and temporary values.
        /// </summary>
        /// <returns>
        /// A dictionary where:
        /// <list type="bullet">
        /// <item><description>The key is the name of the desire as a <see cref="string"/>.</description></item>
        /// <item><description>
        /// The value is a tuple containing:
        /// <list type="bullet">
        /// <item><description><see cref="int"/>: The weight of the desire.</description></item>
        /// <item><description><see cref="double"/>: The temporary value of the desire.</description></item>
        /// </list>
        /// </description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method iterates through all desires in the internal <see cref="Desires"/> collection and extracts
        /// their weights and temporary values into a dictionary. The temporary value reflects the current state
        /// of the desire, which may have been updated during the simulation.
        /// </remarks>
        public Dictionary<string, (int, double)> GetCurrentDesireValue_Linear()
        {
            // Initialize a dictionary to store the desire names and their corresponding weight and temporary values.
            Dictionary<string, (int, double)> desireName_ValueBeforeApply_Dict = new Dictionary<string, (int, double)>();

            // Iterate over all desires in the Desires collection.
            foreach (var calcDesire in Desires.Values)
            {
                // Extract the name, weight, and temporary value of the desire.
                // Convert the weight to an integer and the temporary value to a double.
                desireName_ValueBeforeApply_Dict[calcDesire.Name] = (((int)calcDesire.Weight), (double)calcDesire.TempValue);
            }

            // Return the dictionary containing the extracted desire values.
            return desireName_ValueBeforeApply_Dict;
        }

        /// <summary>
        /// Calculates the total deviation, weight sum, and updated desire values for a given duration, 
        /// considering satisfaction values and decay rates in a linear fashion.
        /// </summary>
        /// <param name="duration">The duration for which the calculation is performed.</param>
        /// <param name="satisfactionvaluesDict">
        /// A dictionary mapping desire IDs to satisfaction values. These values influence the 
        /// updating of desire states during the duration.
        /// </param>
        /// <param name="thoughtstring">An optional output parameter, ignored in this implementation.</param>
        /// <param name="optionalList">
        /// An optional dictionary mapping desire names to their weight and temporary values. If provided 
        /// and its count matches the desires collection, it overrides the current desire values.
        /// </param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><description><see cref="double"/> totalDeviation: The total weighted deviation over the duration.</description></item>
        /// <item><description><see cref="double"/> WeightSum: The total weight of all desires with positive satisfaction values.</description></item>
        /// <item><description>
        /// A dictionary where keys are desire names and values are tuples containing:
        /// <list type="bullet">
        /// <item><description>Weight of the desire (<see cref="int"/>).</description></item>
        /// <item><description>Updated value of the desire (<see cref="double"/>).</description></item>
        /// </list>
        /// </description></item>
        /// <item><description><see cref="int"/> realDuration: The actual duration of the calculation (same as input).</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method iterates through all desires, applying either a satisfaction value or decay rate
        /// to update their states and calculate the weighted deviation.
        /// </remarks>

        private (double totalDeviation, double WeightSum, Dictionary<string, (int, double)> desireName_ValueAfterApply, int realDuration) CalcTotalDeviation_Linear(int duration, Dictionary<int,double> satisfactionvaluesDict, out string? thoughtstring)
        {
            // Initialize the total weighted deviation and satisfaction value (after the apply) dictionary
            Dictionary<string, (int, double)> desireValueAfterApply = new Dictionary<string, (int, double)>();
            double totalDeviation = 0;

            StringBuilder? sb = null; 
            var makeThoughts = _calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile); 
            if (makeThoughts)
            {
                sb = new StringBuilder(_calcRepo.CalcParameters.CSVCharacter);
            }

            // Initialize the weight sum and satisfaction value dictionary
            var satisfactionValueDictionary = satisfactionvaluesDict;
            double weight_sum = 0;

            // Iterate through all desires in the Desires collection
            foreach (var calcDesire in Desires.Values)
            {
                // Extract the desire ID and name
                var desireID = calcDesire.DesireID;
                var DesireName = calcDesire.Name;

                // Retrieve the satisfaction value for the current desire, if present
                satisfactionValueDictionary.TryGetValue(desireID, out var satisfactionvalueDBL);

                // Get the decay rate, temporary value, and weight of the desire
                var decayrate = (double)calcDesire.DecayRate;
                var currentValueDBL = (double)calcDesire.TempValue;
                var weightDBL = (double)calcDesire.Weight;

                // Include weight in the weight sum if satisfaction value is positive
                if (satisfactionvalueDBL > 0)
                {
                    weight_sum += weightDBL;
                }

                // Initialize variables for deviation and updated value calculation
                double deviration = 0;
                double updateValue = currentValueDBL;

                // Update values based on satisfaction value or decay rate
                if (satisfactionvalueDBL > 0)
                {
                    // Apply satisfaction incrementally over the duration
                    for (var i = 0; i < duration; i++)
                    {
                        updateValue = Math.Min(1, updateValue + (satisfactionvalueDBL / duration));
                        deviration += (1 - updateValue);
                    }
                }
                else
                {
                    // Apply decay incrementally over the duration
                    for (var i = 0; i < duration; i++)
                    {
                        updateValue *= decayrate;
                        deviration += (1 - updateValue);
                    }
                }

                // Store the updated desire value and weight in the resulting dictionary
                desireValueAfterApply[calcDesire.Name] = (((int)calcDesire.Weight), updateValue);

                // Calculate the weighted deviation and add it to the total deviation
                var weightedDeviration = deviration * weightDBL;
                totalDeviation += weightedDeviration;

                if (sb != null) 
                {
                    var deviation = (((1 - currentValueDBL) + (1 - updateValue)) / 2) * 100;
                    sb.Append(calcDesire.Name);
                    sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append("'");
                    sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                    sb.Append("*");
                    sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                    sb.Append("*");
                    sb.Append(calcDesire.Weight.ToString("0#.#", Config.CultureInfo));
                    sb.Append(_calcRepo.CalcParameters.CSVCharacter);
                    sb.Append(deviration.ToString("0#.#", Config.CultureInfo));
                    sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append(" ");
                }
            }

            // Ensure the weight sum is at least 1 to prevent division errors in subsequent calculations
            weight_sum = weight_sum < 1 ? 1 : weight_sum;
           
            thoughtstring = sb?.ToString() ?? null;

            // Return the calculated values
            return (totalDeviation, weight_sum, desireValueAfterApply, duration);
        }


        private decimal CalcTotalDeviation(out string? thoughtstring) {
            decimal totalDeviation = 0;
            StringBuilder? sb = null;
            var makeThoughts = _calcRepo.CalcParameters.IsSet(CalcOption.ThoughtsLogfile);
            if (makeThoughts) {
                sb = new StringBuilder(_calcRepo.CalcParameters.CSVCharacter);
            }

            foreach (var calcDesire in Desires.Values) {
                if (calcDesire.TempValue < calcDesire.Threshold || calcDesire.TempValue > 1) {
                    var deviation = (1 - calcDesire.TempValue) * 100;
                    var desirevalue = deviation * deviation * calcDesire.Weight;
                    totalDeviation += desirevalue;
                    if (sb!=null) {
                        sb.Append(calcDesire.Name);
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append("'");
                        sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                        sb.Append("*");
                        sb.Append(deviation.ToString("0#.#", Config.CultureInfo));
                        sb.Append("*");
                        sb.Append(calcDesire.Weight.ToString("0#.#", Config.CultureInfo));
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter);
                        sb.Append(desirevalue.ToString("0#.#", Config.CultureInfo));
                        sb.Append(_calcRepo.CalcParameters.CSVCharacter).Append(" ");
                    }
                }
            }
            if (sb!=null) {
                thoughtstring = sb.ToString();
            }
            else {
                thoughtstring = null;
            }
            //thoughtstring = null;
            return totalDeviation;
        }


        public void CheckForCriticalThreshold([NotNull] CalcPerson person, [NotNull] TimeStep time, [NotNull] FileFactoryAndTracker fft,
                                              [NotNull] HouseholdKey householdKey) {
            if ( time.ExternalStep < 0 &&
                !time.ShowSettling) {
                return;
            }

            var builder = new StringBuilder();
            foreach (var calcDesire in Desires) {
                if (calcDesire.Value.CriticalThreshold > 0) {
                    if (calcDesire.Value.Value < calcDesire.Value.CriticalThreshold) {
                        builder.Append("1");
                    }
                    else {
                        builder.Append("0");
                    }
                    builder.Append(_calcRepo.CalcParameters.CSVCharacter);
                }
            }
            if (builder.Length > 0) {
                var sb = new StringBuilder();
                _dsc.GenerateDateStampForTimestep(time, sb);

                if (_sw == null) {
                    var personNumber = _persons.Count;
                    _persons.Add(new Tuple<string, HouseholdKey>(person.Name, householdKey), personNumber);
                    _sw = fft.MakeFile<StreamWriter>(
                        "CriticalThresholdViolations." + householdKey + "." + person + ".csv",
                        "Lists the critical threshold violations for " + person, true,
                        ResultFileID.CriticalThresholdViolations, householdKey,
                        TargetDirectory.Debugging, _calcRepo.CalcParameters.InternalStepsize, CalcOption.CriticalViolations, null,person.MakePersonInformation());
                    var header = _dsc.GenerateDateStampHeader();
                    foreach (var calcDesire in Desires) {
                        if (calcDesire.Value.CriticalThreshold > 0) {
#pragma warning disable CC0039 // Don't concatenate strings in loops
                            header += calcDesire.Value.Name;
                            header += _calcRepo.CalcParameters.CSVCharacter;
#pragma warning restore CC0039 // Don't concatenate strings in loops
                        }
                    }
                    if(_sw==null) {
                        throw new LPGException("SW was null");
                    }
                    _sw.WriteLine(header);
                }
                sb.Append(builder);
                _sw.WriteLine(sb);
            }
        }

        public void CopyOtherDesires([NotNull] CalcPersonDesires otherdesires) {
            foreach (var calcDesire in Desires.Values) {
                if (calcDesire.DecayTime < 100) {
                    calcDesire.Value = 1;
                }
                if (otherdesires.Desires.ContainsKey(calcDesire.DesireID)) {
                    var tmpdes = otherdesires.Desires[calcDesire.DesireID];
                    calcDesire.Value = tmpdes.Value;
                }
            }
            if (_sw == null && otherdesires._sw != null) {
                _sw = otherdesires._sw;
            }
        }

        public bool HasAtLeastOneDesireBelowThreshold([NotNull] ICalcAffordanceBase aff) {
            foreach (var desire in aff.Satisfactionvalues) {
                if (Desires.ContainsKey(desire.DesireID)) {
                    if (Desires[desire.DesireID].Value < Desires[desire.DesireID].Threshold) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
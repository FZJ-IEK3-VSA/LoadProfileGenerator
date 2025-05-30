using Common;
using Common.Extensions;
using System.Collections.Generic;

namespace CalculationEngine.HouseholdElements
{
    /// <summary>
    /// This class helps with calculating the duration of affordance activations and determining whether devices are activated or not.
    /// Both usage probabilities and duration time factors are sampled randomly. In order to keep the simulation consistent, this class
    /// ensures that sampled values are stored, so that subsequent requests for the same person and the same timestep retrieve the same
    /// values.
    /// </summary>
    /// <param name="expectedDuration">expected activity duration</param>
    /// <param name="standardDeviation">standard deviation of the duration</param>
    /// <param name="calcRepo">the CalcRepo containing the random number generators</param>
    /// <param name="affordanceName">the name of the affordance using this class, for error messages</param>
    internal class AffordanceDurationCalculator(int expectedDuration, double standardDeviation, CalcRepo calcRepo, string affordanceName)
    {
        /// <summary>
        /// expectation value for the duration in timesteps
        /// </summary>
        private readonly int _duration = expectedDuration;

        /// <summary>
        /// standard deviation of the duration in timesteps
        /// </summary>
        private readonly double standardDeviation = standardDeviation;

        /// <summary>
        /// the CalcRepo containing the random number generators
        /// </summary>
        private readonly CalcRepo _calcRepo = calcRepo;

        /// <summary>
        /// the name of the affordance this duration calculator belongs to; is used for error messages
        /// </summary>
        private readonly string _affordanceName = affordanceName;

        /// <summary>
        /// stores sampled activation probabilities for each person for each requested timestep
        /// </summary>
        private readonly Dictionary<string, Dictionary<int, double>> _probabilitiesForTimes = [];

        /// <summary>
        /// stores sampled duration factors for each person for each requested timestep
        /// </summary>
        private readonly Dictionary<string, Dictionary<int, double>> _timeFactorsForTimes = [];

        /// <summary>
        /// Creates a copy of this duration calculator, but with another affordance name.
        /// </summary>
        /// <param name="affordanceName">the new affordance name to use</param>
        /// <returns>the new copy of this duration calculator</returns>
        public AffordanceDurationCalculator CloneForOtherAffordance(string affordanceName)
            => new(_duration, standardDeviation, _calcRepo, affordanceName);

        /// <summary>
        /// Get the activation probability for the specified person and timestep.
        /// </summary>
        /// <param name="timestep">the timestep to get the probability for</param>
        /// <param name="personName">the person to get the probability for</param>
        /// <returns>the sampled probability</returns>
        public double GetProbability(TimeStep timestep, string personName)
        {
            return GetValueForPerson(timestep, personName, _probabilitiesForTimes);
        }

        /// <summary>
        /// Gets the duration time factor for the specified person and timestep.
        /// </summary>
        /// <param name="timestep">the timestep to get the time factor for</param>
        /// <param name="personName">the person to get the time factor for</param>
        /// <returns>the sampled time factor</returns>
        public double GetTimeFactor(TimeStep timestep, string personName)
        {
            return GetValueForPerson(timestep, personName, _timeFactorsForTimes);
        }

        /// <summary>
        /// Checks if the specified nested dictionary contains a value for the specified person and timestep already, and returns it.
        /// If the value does not exist yet, it is sampled and stored.
        /// </summary>
        /// <param name="timestep">the timestep to get the value for</param>
        /// <param name="personName">the person to get the value for</param>
        /// <param name="valueDict">the value dictionary to look in</param>
        /// <returns>the sampled value</returns>
        private double GetValueForPerson(TimeStep timestep, string personName, Dictionary<string, Dictionary<int, double>> valueDict)
        {
            // check if there is already a value for this person for this timestep
            if (!valueDict.TryGetValue(personName, out var values) || !values.TryGetValue(timestep.InternalStep, out var value))
            {
                DetermineRandomValues(timestep, personName);
                value = valueDict[personName][timestep.InternalStep];
            }
            // return the appropriate value
            return value;
        }

        /// <summary>
        /// Gets the end of the activation if activated at the specified time by the specified person.
        /// </summary>
        /// <param name="startTime">the timestep to start the activation</param>
        /// <param name="personName">the person activating the affordance</param>
        /// <returns></returns>
        public TimeStep GetEnd(TimeStep startTime, string personName)
        {
            // determine the time the activating person is busy with the affordance
            var factor = GetTimeFactor(startTime, personName);
            int personsteps = CalcProfile.GetNewLengthAfterCompressExpand(_duration, factor);
            return startTime.AddSteps(personsteps);
        }

        /// <summary>
        /// Clears the time factors and probabilities for one person. This should be called whenever an affordance is activated
        /// and the old sampled values are not needed anymore to save space.
        /// </summary>
        /// <param name="personName">the name of the person to clear the values of</param>
        public void ClearForPerson(string personName)
        {
            // clear probabilities and time factors for the activating person only
            _probabilitiesForTimes[personName].Clear();
            _timeFactorsForTimes[personName].Clear();
        }

        /// <summary>
        /// Samples a time factor and probability for the specified person and timestep.
        /// </summary>
        /// <param name="timestep">the timestep to determine the values for</param>
        /// <param name="personName">the person to determine the values for</param>
        private void DetermineRandomValues(TimeStep timestep, string personName)
        {
            // determine both time factors and probabilities for this person and this timestep
            DetermineProbabilities(timestep, personName);
            DetermineTimeFactors(timestep, personName);
        }

        /// <summary>
        /// Checks if a time factor has already been set for this time step, and if not determines it.
        /// </summary>
        /// <param name="time">the time step to set a time factor for</param>
        /// <param name="personName">name of the person for whom a time factor is required</param>
        private void DetermineTimeFactors(TimeStep time, string personName)
        {
            var factorsForPerson = _timeFactorsForTimes.GetOrAddDefault(personName);
            if (!factorsForPerson.ContainsKey(time.InternalStep))
            {
                factorsForPerson[time.InternalStep] = _calcRepo.NormalRandom.NextDouble(1, standardDeviation);
                if (factorsForPerson[time.InternalStep] < 0)
                {
                    throw new DataIntegrityException($"The duration standard deviation on {_affordanceName} is too large: a negative value of " +
                        $"{factorsForPerson[time.InternalStep]} came up. The standard deviation is {standardDeviation}");
                }
            }
        }

        /// <summary>
        /// Checks if a probability has already been set for this time step, and if not determines it.
        /// </summary>
        /// <param name="time">the time step to set probabilities for</param>
        /// <param name="personName">name of the person for whom a probability is required</param>
        private void DetermineProbabilities(TimeStep time, string personName)
        {
            var probsForPerson = _probabilitiesForTimes.GetOrAddDefault(personName);
            if (!probsForPerson.ContainsKey(time.InternalStep))
            {
                probsForPerson[time.InternalStep] = _calcRepo.Rnd.NextDouble();
            }
        }
    }
}

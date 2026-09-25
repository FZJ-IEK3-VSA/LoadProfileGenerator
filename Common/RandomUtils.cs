using Automation.ResultFiles;
using System;

namespace Common
{
    /// <summary>
    /// Helper functions for generating random values.
    /// </summary>
    public static class RandomUtils
    {
        /// <summary>
        /// Maximum number of attempts to generate a random value again if the original value
        /// was not suitable (e.g., negative).
        /// </summary>
        private const int MAX_RANDOM_ATTEMPTS = 100;

        /// <summary>
        /// Checks whether the passed value is within the permitted range.
        /// The range is defined by the allowed deviation around the mean.
        /// </summary>
        /// <param name="value">the value to check</param>
        /// <param name="mean">the expectation value from which the deviation is checked</param>
        /// <param name="allowedDeviation">the allowed deviation around the expectation value</param>
        /// <returns>true if the value is within the range; otherwise, false</returns>
        private static bool IsInRange(double value, double mean, double allowedDeviation)
        {
            // get the absolute difference to the mean
            double difference = Math.Abs(value - mean);
            return difference <= allowedDeviation;
        }

        /// <summary>
        /// Generates a random value using the passed NormalRandom object and the distribution parameters.
        /// Makes sure that the value keeps the specified maximum deviation from the mean. If not, the value
        /// is discarded and another random value is generated, until a valid one is found or the maximum
        /// number of attempts is exceeded.
        /// </summary>
        /// <param name="nr">normal random number generator</param>
        /// <param name="mean">expectation value for the normal random distribution</param>
        /// <param name="standardDeviation">standard deviation for the normal random distribution</param>
        /// <param name="deviation">maximum permitted deviation from the mean for the generated value</param>
        /// <returns>the generated random value</returns>
        /// <exception cref="LPGException">if the maximum number of retries was exceeded</exception>
        public static double GetNormalRandomWithinLimits(NormalRandom nr, double mean, double standardDeviation, double deviation)
        {
            int attempts = 0;
            double value;
            do
            {
                if (attempts > MAX_RANDOM_ATTEMPTS)
                    throw new LPGException($"Could not create a non-negative value for a profile in {MAX_RANDOM_ATTEMPTS} attempts. Standard deviation: {standardDeviation}");
                value = nr.NextDouble(mean, standardDeviation);
                attempts++;
            } while (!IsInRange(value, mean, deviation));
            return value;
        }
    }
}

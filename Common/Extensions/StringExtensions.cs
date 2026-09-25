using Automation;

namespace Common.Extensions
{
    /// <summary>
    /// Defines string extension methods
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// If this string ends with the specified suffix, removes the suffix
        /// and returns the resulting substring.
        /// </summary>
        /// <param name="s">the original string</param>
        /// <param name="suffix">the suffix to remove</param>
        /// <returns>the substring without the suffix</returns>
        public static string RemoveSuffix(this string s, string suffix)
        {
            if (s.EndsWith(suffix))
            {
                return s.Substring(0, s.Length - suffix.Length);
            }
            return s;
        }

        /// <summary>
        /// Creates a new StrGuid from this string. The resulting StrGuid only contains
        /// this string as value and is not guaranteed to be unique.
        /// </summary>
        /// <param name="myguid">the string to turn into a StrGuid</param>
        /// <returns>the new StrGuid containing the string as value</returns>
        public static StrGuid ToStrGuid(this string myguid)
        {
            return StrGuid.FromString(myguid);
        }
    }
}

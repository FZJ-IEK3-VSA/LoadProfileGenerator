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

        public static StrGuid ToStrGuid(this string myguid)
        {
            return StrGuid.FromString(myguid);
        }
    }
}

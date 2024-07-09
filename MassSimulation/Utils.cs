using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    internal static class Utils
    {
        // TODO: use the random object used in LPG if possible
        internal static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void print(int rank, string s)
        {
            Console.WriteLine(rank + ": " + s);
        }

        public static void print(int timestep, int rank, string s)
        {
            Console.WriteLine(timestep + ": " + rank + ": " + s);
        }
    }

    static class LinqExtensions
    {
        /// <summary>
        /// Splits an enumerable into parts of equal size (if possible). Uses modulo to distribute
        /// the elements in a round-robin fashion.
        /// </summary>
        /// <typeparam name="T">type of the elements</typeparam>
        /// <param name="list">enumerable to split</param>
        /// <param name="parts">number of parts</param>
        /// <returns>an enumerable containing the parts, which are themselves enumerables</returns>
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
}

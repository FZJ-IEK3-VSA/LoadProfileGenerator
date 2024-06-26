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
}

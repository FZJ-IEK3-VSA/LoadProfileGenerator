using System.Collections.Generic;
using System.Linq;

namespace Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Like the LINQ Zip method, but continues until both sequences are exhausted, filling in default values for the shorter sequence.
        /// </summary>
        /// <typeparam name="TFirst">Type of the elements in the first enumerable</typeparam>
        /// <typeparam name="TSecond">Type of the elements in the second enumerable</typeparam>
        /// <param name="first">the first enumerable</param>
        /// <param name="second">the second enumerable</param>
        /// <param name="defaultFirst">default value to use when the first enumerable is exhausted</param>
        /// <param name="defaultSecond">default value to use when the second enumerable is exhausted</param>
        /// <returns>a new enumerable containing tuples of one element of each input enumerable</returns>
        public static IEnumerable<(TFirst, TSecond)> ZipLongest<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, TFirst defaultFirst = default, TSecond defaultSecond = default)
        {
            using var firstEnumerator = first.GetEnumerator();
            using var secondEnumerator = second.GetEnumerator();

            bool firstHasNext = firstEnumerator.MoveNext();
            bool secondHasNext = secondEnumerator.MoveNext();
            while (firstHasNext || secondHasNext)
            {
                yield return (
                    firstHasNext ? firstEnumerator.Current : defaultFirst,
                    secondHasNext ? secondEnumerator.Current : defaultSecond
                );

                firstHasNext = firstEnumerator.MoveNext();
                secondHasNext = secondEnumerator.MoveNext();
            }
        }

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

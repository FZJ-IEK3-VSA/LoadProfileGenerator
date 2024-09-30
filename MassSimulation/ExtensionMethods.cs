using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassSimulation
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified key. If the key is not contained in the dictionary,
        /// creates a new default instance of the value type and adds it to the dictionary with the specified key.
        /// </summary>
        /// <typeparam name="TKey">dictionary key type</typeparam>
        /// <typeparam name="TValue">dictionary value type</typeparam>
        /// <param name="dict">the dictionary to get a value from</param>
        /// <param name="key">the key of the value to get</param>
        /// <returns></returns>
        public static TValue GetOrAddDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) where TKey : notnull where TValue : new()
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = new();
                dict[key] = value;
            }
            return value;
        }
    }
}

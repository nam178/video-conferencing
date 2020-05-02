using System;
using System.Collections.Generic;

namespace MediaServer.WebRtc.MediaRouting
{
    static class DictionaryExtensions
    {
        public static void Add<K, V>(this Dictionary<K, HashSet<V>> dict, K key, V value)
        {
            if(false == dict.ContainsKey(key))
            {
                dict[key] = new HashSet<V> { value };
            }
            else
            {
                if(dict[key].Contains(value))
                {
                    throw new ArgumentException($"Duplicate value for {typeof(K).Name}={key}, {typeof(V).Name}={value}");
                }
                dict[key].Add(value);
            }
        }

        public static void Remove<K, V>(this Dictionary<K, HashSet<V>> dict, K key, V value)
        {
            if(false == dict.ContainsKey(key))
                return;

            dict[key].Remove(value);
            if(dict[key].Count == 0)
            {
                dict.Remove(key);
            }
        }
    }
}

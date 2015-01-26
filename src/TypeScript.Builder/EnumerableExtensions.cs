namespace TypeScript.Factory
{
    using System;
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        public static void Apply<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void Apply<K, V>(this IEnumerable<KeyValuePair<K, V>> source, Action<K, V> action)
        {
            foreach (var kvp in source)
            {
                action(kvp.Key, kvp.Value);
            }
        }
    }
}

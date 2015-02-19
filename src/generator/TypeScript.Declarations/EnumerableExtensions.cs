namespace TypeScript.Declarations
{
    using System;
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Calls an action for each item in the source. Successfully one-lines foreach.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable's items.</typeparam>
        /// <param name="source">The enumerable.</param>
        /// <param name="onItem">Called for each item.</param>
        public static void Apply<T>(this IEnumerable<T> source, Action<T> onItem)
        {
            foreach (var item in source)
            {
                onItem(item);
            }
        }

        /// <summary>
        /// Applyies actions on objects in enumerator.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable's items.</typeparam>
        /// <param name="source">The enumerable.</param>
        /// <param name="onItem">Called for each item.</param>
        /// <param name="onSeparator">Called between items.</param>
        public static void ApplyWithSeparators<T>(this IEnumerable<T> source, Action<T> onItem, Action onSeparator)
        {
            var e = source.GetEnumerator();
            if (e.MoveNext())
            {
                e.ApplyWithSeparators(onItem, onSeparator);
            }
        }

        /// <summary>
        /// Applyies actions on objects in enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable's items.</typeparam>
        /// <param name="source">The enumerable.</param>
        /// <param name="beforeFirst">Called if the enumerable contains items, before the first item.</param>
        /// <param name="onItem">Called for each item.</param>
        /// <param name="onSeparator">Called between items.</param>
        /// <param name="afterLast">Called if the enumerable contains items, after the last item.</param>
        public static void ApplyWithSeparators<T>(this IEnumerable<T> source, Action beforeFirst, Action<T> onItem, Action onSeparator, Action afterLast)
        {
            var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
            {
                if (beforeFirst != null)
                {
                    beforeFirst();
                }

                enumerator.ApplyWithSeparators(onItem, onSeparator);
                if (afterLast != null)
                {
                    afterLast();
                }
            }
        }

        /// <summary>
        /// Applyies actions on objects in enumerator.
        /// </summary>
        /// <typeparam name="T">The type of the enumerator's items.</typeparam>
        /// <param name="source">The enumerable.</param>
        /// <param name="onItem">Called for each item.</param>
        /// <param name="onSeparator">Called between items.</param>
        private static void ApplyWithSeparators<T>(this IEnumerator<T> source, Action<T> onItem, Action onSeparator)
        {
            onItem(source.Current);
            while (source.MoveNext())
            {
                onSeparator();
                onItem(source.Current);
            }
        }
    }
}

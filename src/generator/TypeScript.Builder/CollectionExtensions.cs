namespace TypeScript.Factory
{
    using System.Collections.Generic;

    internal static class CollectionExtensions
    {
        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }

            return collection;
        }

        public static ICollection<T> RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Remove(item);
            }

            return collection;
        }
    }
}

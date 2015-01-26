using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Libclang.Core.Ast;

namespace Libclang.Core.Common
{
    public static class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            var visited = new HashSet<TKey>();

            return source.Where(element => visited.Add(selector(element)));
        }

        public static bool SequenceEqual<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, TSource, bool> comparer)
        {
            using (IEnumerator<TSource> enumerator1 = first.GetEnumerator())
            using (IEnumerator<TSource> enumerator2 = second.GetEnumerator())
            {
                while (enumerator1.MoveNext())
                {
                    if (!(enumerator2.MoveNext() && comparer(enumerator1.Current, enumerator2.Current)))
                    {
                        return false;
                    }
                }

                if (enumerator2.MoveNext())
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> list, int chunkSize)
        {
            int i = 0;
            var chunks = from name in list
                group name by i++/chunkSize
                into part
                select part.AsEnumerable();
            return chunks;
        }

        public static bool IsEqualToAny(this object obj, params object[] elements)
        {
            return elements.Contains(obj);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                collection.Add(element);
            }
        }
    }
}

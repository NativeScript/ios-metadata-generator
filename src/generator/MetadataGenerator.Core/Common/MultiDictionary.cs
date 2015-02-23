using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace MetadataGenerator.Core.Common
{
    public class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ICollection<TValue>>>
    {
        private readonly Dictionary<TKey, ICollection<TValue>> items = new Dictionary<TKey, ICollection<TValue>>();

        public IEnumerable<TKey> Keys
        {
            get { return items.Keys; }
        }

        public void Add(TKey key, TValue value)
        {
            if (!items.ContainsKey(key))
            {
                items.Add(key, new List<TValue>());
            }

            items[key].Add(value);
        }

        public void AddMany(TKey key, IEnumerable<TValue> values)
        {
            if (!items.ContainsKey(key))
            {
                items.Add(key, new List<TValue>());
            }

            foreach (var value in values)
            {
                items[key].Add(value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return items.ContainsKey(key);
        }

        public ICollection<TValue> this[TKey index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        public IEnumerator<KeyValuePair<TKey, ICollection<TValue>>> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}

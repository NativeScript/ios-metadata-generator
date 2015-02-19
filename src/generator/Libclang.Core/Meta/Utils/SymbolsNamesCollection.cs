using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Meta.Utils
{
    public class SymbolsNamesCollection
    {
        private Dictionary<Type, HashSet<string>> collection;

        public void Add(Type metaType, params string[] names)
        {
            if (!this.collection.ContainsKey(metaType))
            {
                this.collection.Add(metaType, new HashSet<string>());
            }

            foreach (string name in names)
            {
                collection[metaType].Add(name);
            }
        }

        public bool Contains(string name, Type metaType)
        {
            return this.collection.ContainsKey(metaType) && this.collection[metaType].Contains(name);
        }

        public SymbolsNamesCollection()
        {
            this.collection = new Dictionary<Type, HashSet<string>>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
{
    internal class RemoveDuplicateMembersFilter : BaseMetaFilter
    {
        private static readonly IEqualityComparer<MethodMeta> methodComparer = new MethodEqualSignatureComparer();
        private static readonly IEqualityComparer<PropertyMeta> propertyComparer = new PropertyEqualSignatureComparer();

        public RemoveDuplicateMembersFilter()
            : this(null)
        {
        }

        public RemoveDuplicateMembersFilter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<MetaContainer, BaseClassMeta, BaseClassMeta> ActionForEachPair
        {
            get { return this.ProcessPredecessorSuccessorPair; }
        }

        protected void ProcessPredecessorSuccessorPair(MetaContainer metaContainer, BaseClassMeta predecessor,
            BaseClassMeta successor)
        {
            // Extract equal methods
            HashSet<MethodMeta> methodsToRemove = new HashSet<MethodMeta>();
            foreach (MethodMeta method in successor.Methods)
            {
                if (predecessor.Methods.Contains(method, methodComparer))
                {
                    methodsToRemove.Add(method);
                }
            }

            // Extract equal properties
            HashSet<PropertyMeta> propertiesToRemove = new HashSet<PropertyMeta>();
            foreach (PropertyMeta property in successor.Properties)
            {
                if (predecessor.Properties.Contains(property, propertyComparer))
                {
                    propertiesToRemove.Add(property);
                }
            }

            // Remove duplicates
            foreach (MethodMeta method in methodsToRemove)
            {
                successor.Methods.Remove(method);
                this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, method.Selector, method.ExtendedEncoding,
                    predecessor.Name);
            }
            foreach (PropertyMeta property in propertiesToRemove)
            {
                successor.Properties.Remove(property);
                this.Log("Property: {0}.{1} [ {2} ] -> {3}", successor.Name, property.Name, property.ExtendedEncoding,
                    predecessor.Name);
            }
        }
    }

    internal class MethodEqualSignatureComparer : IEqualityComparer<MethodMeta>
    {
        public bool Equals(MethodMeta x, MethodMeta y)
        {
            return (x.IsStatic == y.IsStatic && x.Selector == y.Selector && x.ExtendedEncoding == y.ExtendedEncoding);
        }

        public int GetHashCode(MethodMeta obj)
        {
            return obj.Selector.GetHashCode() ^ obj.ExtendedEncoding.GetHashCode() ^ obj.IsStatic.GetHashCode();
        }
    }

    internal class PropertyEqualSignatureComparer : IEqualityComparer<PropertyMeta>
    {
        public bool Equals(PropertyMeta x, PropertyMeta y)
        {
            bool gettersComparison = (x.Getter == null) == (y.Getter == null);
            bool settersComparison = (x.Setter == null) == (y.Setter == null);
            return (x.Name == y.Name && x.ExtendedEncoding == y.ExtendedEncoding && gettersComparison &&
                    settersComparison);
        }

        public int GetHashCode(PropertyMeta obj)
        {
            bool hasGetter = (obj.Getter != null);
            bool hasSetter = (obj.Setter != null);
            return obj.Name.GetHashCode() ^ obj.ExtendedEncoding.GetHashCode() ^ hasGetter.GetHashCode() ^
                   hasSetter.GetHashCode();
        }
    }
}

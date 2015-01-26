using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
{
    internal class MarkMembersWithSameJsNamesInHierarchyFilter : BaseMetaFilter
    {
        private static readonly IEqualityComparer<MemberMeta> membersComparer = new EqualJsNameComparer();

        public MarkMembersWithSameJsNamesInHierarchyFilter()
            : this(null)
        {
        }

        public MarkMembersWithSameJsNamesInHierarchyFilter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<MetaContainer, Meta, string> ActionForEach
        {
            get { return this.ProcessSingleMeta; }
        }

        protected override Action<MetaContainer, BaseClassMeta, BaseClassMeta> ActionForEachPair
        {
            get { return this.ProcessPredecessorSuccessorPair; }
        }

        protected void ProcessSingleMeta(MetaContainer metaContainer, Meta meta, string key)
        {
            if (meta is InterfaceMeta)
            {
                InterfaceMeta interfaceMeta = (InterfaceMeta) meta;
                // get duplicates from categories
                IEnumerable<IGrouping<string, MemberMeta>> instanceDuplicates = interfaceMeta.InstanceMethods
                    .Union((IEnumerable<MemberMeta>) interfaceMeta.Properties)
                    .Union(interfaceMeta.Categories.SelectMany(c => c.InstanceMethods))
                    .Union(interfaceMeta.Categories.SelectMany(c => (IEnumerable<MemberMeta>) c.Properties))
                    .GroupBy(m => m.JSName)
                    .Where(g => g.Count() > 1);

                IEnumerable<IGrouping<string, MethodMeta>> staticDuplicates = interfaceMeta.StaticMethods
                    .Union(interfaceMeta.Categories.SelectMany(c => c.StaticMethods))
                    .GroupBy(m => m.JSName)
                    .Where(g => g.Count() > 1);

                foreach (IGrouping<string, MemberMeta> group in instanceDuplicates)
                {
                    int index = 0;
                    foreach (MemberMeta memberMeta in group)
                    {
                        if (index == 0)
                        {
                            // Only the first local duplicate is marked to have duplicates
                            memberMeta.HasJsNameDuplicateInHierarchy = true;
                        }
                        else
                        {
                            // All but one members are marked as local duplicates of this member
                            memberMeta.IsLocalJsNameDuplicate = true;
                        }
                        if (memberMeta is PropertyMeta)
                        {
                            this.Log("Property: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name, memberMeta.Name,
                                memberMeta.ExtendedEncoding, interfaceMeta.Name);
                        }
                        else
                        {
                            this.Log("Method: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name,
                                ((MethodMeta) memberMeta).Selector, memberMeta.ExtendedEncoding, interfaceMeta.Name);
                        }
                        index++;
                    }
                }

                foreach (IGrouping<string, MemberMeta> group in staticDuplicates)
                {
                    foreach (MemberMeta memberMeta in group)
                    {
                        memberMeta.HasJsNameDuplicateInHierarchy = true;
                        this.Log("Method: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name,
                            ((MethodMeta) memberMeta).Selector, memberMeta.ExtendedEncoding, interfaceMeta.Name);
                    }
                }
            }
        }

        protected void ProcessPredecessorSuccessorPair(MetaContainer metaContainer, BaseClassMeta predecessor,
            BaseClassMeta successor)
        {
            // Mark methods
            foreach (MethodMeta method in successor.Methods)
            {
                bool isLocalDuplicate = method.IsLocalJsNameDuplicate.HasValue && method.IsLocalJsNameDuplicate.Value;
                if (!isLocalDuplicate &&
                    predecessor.Methods.Contains(method, membersComparer) ||
                    predecessor.Properties.Contains((MemberMeta) method, membersComparer))
                {
                    method.HasJsNameDuplicateInHierarchy = true;
                    this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, method.Selector, method.ExtendedEncoding,
                        predecessor.Name);
                }
            }

            // Mark properties
            foreach (PropertyMeta property in successor.Properties)
            {
                bool isLocalDuplicate = property.IsLocalJsNameDuplicate.HasValue &&
                                        property.IsLocalJsNameDuplicate.Value;
                if (!isLocalDuplicate &&
                    predecessor.Methods.Contains((MemberMeta) property, membersComparer) ||
                    predecessor.Properties.Contains((MemberMeta) property, membersComparer))
                {
                    property.HasJsNameDuplicateInHierarchy = true;
                    this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, property.Name, property.ExtendedEncoding,
                        predecessor.Name);
                }
            }
        }
    }

    internal class EqualJsNameComparer : IEqualityComparer<MemberMeta>
    {
        public bool Equals(MemberMeta x, MemberMeta y)
        {
            if (x.JSName == y.JSName)
            {
                if (x is PropertyMeta && y is PropertyMeta)
                {
                    return true;
                }
                if (x is PropertyMeta && y is MethodMeta)
                {
                    return !((MethodMeta) y).IsStatic;
                }
                if (x is MethodMeta && y is PropertyMeta)
                {
                    return !((MethodMeta) x).IsStatic;
                }
                if (x is MethodMeta && y is MethodMeta)
                {
                    return ((MethodMeta) x).IsStatic == ((MethodMeta) y).IsStatic;
                }
            }

            return false;
        }

        public int GetHashCode(MemberMeta obj)
        {
            int hashCode = obj.JSName.GetHashCode();
            if (obj is MethodMeta)
            {
                hashCode ^= ((MethodMeta) obj).IsStatic.GetHashCode();
            }
            return hashCode;
        }
    }
}

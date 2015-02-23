using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Ast;
using Libclang.Core.Generator;

namespace Libclang.Core.Meta.Filters
{
    internal class MarkMembersWithSameJsNamesInHierarchyFilter : BaseMetaFilter
    {
        private static readonly IEqualityComparer<BaseDeclaration> membersComparer = new EqualJsNameComparer();

        public MarkMembersWithSameJsNamesInHierarchyFilter()
            : this(null)
        {
        }

        public MarkMembersWithSameJsNamesInHierarchyFilter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<ModuleDeclarationsContainer, IDeclaration> ActionForEach
        {
            get { return this.ProcessSingleMeta; }
        }

        protected override Action<ModuleDeclarationsContainer, BaseClass, BaseClass> ActionForEachPair
        {
            get { return this.ProcessPredecessorSuccessorPair; }
        }

        protected void ProcessSingleMeta(ModuleDeclarationsContainer metaContainer, IDeclaration meta)
        {
            if (meta is InterfaceDeclaration)
            {
                InterfaceDeclaration interfaceMeta = (InterfaceDeclaration)meta;
                // get duplicates from categories
                IEnumerable<IGrouping<string, BaseDeclaration>> instanceDuplicates = interfaceMeta.InstanceMethods()
                    .Union((IEnumerable<BaseDeclaration>)interfaceMeta.Properties)
                    .Union(interfaceMeta.Categories.SelectMany(c => c.InstanceMethods()))
                    .Union(interfaceMeta.Categories.SelectMany(c => (IEnumerable<BaseDeclaration>)c.Properties))
                    .GroupBy(m => m.GetJSName())
                    .Where(g => g.Count() > 1);

                IEnumerable<IGrouping<string, MethodDeclaration>> staticDuplicates = interfaceMeta.StaticMethods()
                    .Union(interfaceMeta.Categories.SelectMany(c => c.StaticMethods()))
                    .GroupBy(m => m.GetJSName())
                    .Where(g => g.Count() > 1);

                foreach (IGrouping<string, BaseDeclaration> group in instanceDuplicates)
                {
                    int index = 0;
                    foreach (BaseDeclaration memberMeta in group)
                    {
                        PropertyDeclaration propertyMeta = memberMeta as PropertyDeclaration;
                        MethodDeclaration methodMeta = memberMeta as MethodDeclaration;

                        if (index == 0)
                        {
                            // Only the first local duplicate is marked to have duplicates
                            if (propertyMeta != null)
                                propertyMeta.SetHasJsNameDuplicateInHierarchy(true);
                            else
                                methodMeta.SetHasJsNameDuplicateInHierarchy(true);
                        }
                        else
                        {
                            // All but one members are marked as local duplicates of this member
                            if (propertyMeta != null)
                                propertyMeta.SetIsLocalJsNameDuplicate(true);
                            else
                                methodMeta.SetIsLocalJsNameDuplicate(true);
                        }

                        if (propertyMeta != null)
                        {
                            this.Log("Property: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name, memberMeta.Name,
                                propertyMeta.GetExtendedEncoding(), interfaceMeta.Name);
                        }
                        else
                        {
                            this.Log("Method: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name,
                                methodMeta.Selector, methodMeta.GetExtendedEncoding(), interfaceMeta.Name);
                        }
                        index++;
                    }
                }

                foreach (IGrouping<string, MethodDeclaration> group in staticDuplicates)
                {
                    foreach (MethodDeclaration memberMeta in group)
                    {
                        memberMeta.SetHasJsNameDuplicateInHierarchy(true);
                        this.Log("Method: {0}.{1} [ {2} ] -> {3}", interfaceMeta.Name,
                            (memberMeta as MethodDeclaration).Selector, (memberMeta as MethodDeclaration).GetExtendedEncoding(), interfaceMeta.Name);
                    }
                }
            }
        }

        protected void ProcessPredecessorSuccessorPair(ModuleDeclarationsContainer metaContainer, BaseClass predecessor,
            BaseClass successor)
        {
            // Mark methods
            foreach (MethodDeclaration method in successor.Methods)
            {
                bool isLocalDuplicate = method.GetIsLocalJsNameDuplicate().GetValueOrDefault();
                if (!isLocalDuplicate &&
                    predecessor.Methods.Contains(method, membersComparer) ||
                    predecessor.Properties.Contains((BaseDeclaration) method, membersComparer))
                {
                    method.SetHasJsNameDuplicateInHierarchy(true);
                    this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, method.Selector, method.GetExtendedEncoding(),
                        predecessor.Name);
                }
            }

            // Mark properties
            foreach (PropertyDeclaration property in successor.Properties)
            {
                bool isLocalDuplicate = property.GetIsLocalJsNameDuplicate().GetValueOrDefault();
                if (!isLocalDuplicate &&
                    predecessor.Methods.Contains((BaseDeclaration) property, membersComparer) ||
                    predecessor.Properties.Contains((BaseDeclaration) property, membersComparer))
                {
                    property.SetHasJsNameDuplicateInHierarchy(true);
                    this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, property.Name, property.GetExtendedEncoding(),
                        predecessor.Name);
                }
            }
        }
    }

    internal class EqualJsNameComparer : IEqualityComparer<BaseDeclaration>
    {
        public bool Equals(BaseDeclaration x, BaseDeclaration y)
        {
            if (x.GetJSName() == y.GetJSName())
            {
                if (x is PropertyDeclaration && y is PropertyDeclaration)
                {
                    return true;
                }
                if (x is PropertyDeclaration && y is MethodDeclaration)
                {
                    return !((MethodDeclaration) y).IsStatic;
                }
                if (x is MethodDeclaration && y is PropertyDeclaration)
                {
                    return !((MethodDeclaration) x).IsStatic;
                }
                if (x is MethodDeclaration && y is MethodDeclaration)
                {
                    return ((MethodDeclaration) x).IsStatic == ((MethodDeclaration) y).IsStatic;
                }
            }

            return false;
        }

        public int GetHashCode(BaseDeclaration obj)
        {
            int hashCode = obj.GetJSName().GetHashCode();
            if (obj is MethodDeclaration)
            {
                hashCode ^= ((MethodDeclaration) obj).IsStatic.GetHashCode();
            }
            return hashCode;
        }
    }
}

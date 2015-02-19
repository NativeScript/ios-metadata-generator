using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Ast;

namespace Libclang.Core.Meta.Filters
{
    internal class RemoveDuplicateMembersFilter : BaseMetaFilter
    {
        private static readonly IEqualityComparer<MethodDeclaration> methodComparer = new MethodEqualSignatureComparer();
        private static readonly IEqualityComparer<PropertyDeclaration> propertyComparer = new PropertyEqualSignatureComparer();

        public RemoveDuplicateMembersFilter()
            : this(null)
        {
        }

        public RemoveDuplicateMembersFilter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<ModuleDeclarationsContainer, BaseClass, BaseClass> ActionForEachPair
        {
            get { return this.ProcessPredecessorSuccessorPair; }
        }

        protected void ProcessPredecessorSuccessorPair(ModuleDeclarationsContainer metaContainer, BaseClass predecessor,
            BaseClass successor)
        {
            // Extract equal methods
            HashSet<MethodDeclaration> methodsToRemove = new HashSet<MethodDeclaration>();
            foreach (MethodDeclaration method in successor.Methods)
            {
                if (predecessor.Methods.Contains(method, methodComparer))
                {
                    methodsToRemove.Add(method);
                }
            }

            // Extract equal properties
            HashSet<PropertyDeclaration> propertiesToRemove = new HashSet<PropertyDeclaration>();
            foreach (PropertyDeclaration property in successor.Properties)
            {
                if (predecessor.Properties.Contains(property, propertyComparer))
                {
                    propertiesToRemove.Add(property);
                }
            }

            // Remove duplicates
            foreach (MethodDeclaration method in methodsToRemove)
            {
                successor.Methods.Remove(method);
                this.Log("Method: {0}.{1} [ {2} ] -> {3}", successor.Name, method.Selector, method.GetExtendedEncoding(),
                    predecessor.Name);
            }
            foreach (PropertyDeclaration property in propertiesToRemove)
            {
                successor.Properties.Remove(property);
                this.Log("Property: {0}.{1} [ {2} ] -> {3}", successor.Name, property.Name, property.GetExtendedEncoding(),
                    predecessor.Name);
            }
        }
    }

    internal class MethodEqualSignatureComparer : IEqualityComparer<MethodDeclaration>
    {
        public bool Equals(MethodDeclaration x, MethodDeclaration y)
        {
            return x.IsStatic == y.IsStatic && x.Selector == y.Selector && Enumerable.SequenceEqual(x.GetExtendedEncoding(), y.GetExtendedEncoding());
        }

        public int GetHashCode(MethodDeclaration obj)
        {
            return obj.Selector.GetHashCode() ^ obj.GetExtendedEncoding().GetHashCode() ^ obj.IsStatic.GetHashCode();
        }
    }

    internal class PropertyEqualSignatureComparer : IEqualityComparer<PropertyDeclaration>
    {
        public bool Equals(PropertyDeclaration x, PropertyDeclaration y)
        {
            bool gettersComparison = (x.Getter == null) == (y.Getter == null);
            bool settersComparison = (x.Setter == null) == (y.Setter == null);
            return (x.Name == y.Name && x.GetExtendedEncoding() == y.GetExtendedEncoding() && gettersComparison &&
                    settersComparison);
        }

        public int GetHashCode(PropertyDeclaration obj)
        {
            bool hasGetter = (obj.Getter != null);
            bool hasSetter = (obj.Setter != null);
            return obj.Name.GetHashCode() ^ obj.GetExtendedEncoding().GetHashCode() ^ hasGetter.GetHashCode() ^
                   hasSetter.GetHashCode();
        }
    }
}

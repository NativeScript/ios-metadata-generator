using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MetadataGenerator.Core.Meta.Filters
{
    internal class RemoveCategoriesFilter : IMetaFilter
    {
        public RemoveCategoriesFilter()
        {
        }

        public RemoveCategoriesFilter(IEnumerable<ModuleDeclarationsContainer> containers)
        {
            this.containers = containers;
        }

        private readonly IEnumerable<ModuleDeclarationsContainer> containers;

        public void Filter(ModuleDeclarationsContainer metaContainer)
        {
            List<CategoryDeclaration> categoriesToRemove = new List<CategoryDeclaration>();

            foreach (CategoryDeclaration category in metaContainer.OfType<CategoryDeclaration>())
            {
                InterfaceDeclaration owner = category.ExtendedInterface;
                if (metaContainer.Contains(owner) ||
                    (this.containers != null && this.containers.Any(c => c.Contains(category))))
                {
                    this.ExtendOwner(owner, category);
                    categoriesToRemove.Add(category);
                }
            }

            foreach (CategoryDeclaration category in categoriesToRemove)
            {
                metaContainer.Remove(category);
            }
        }

        private void ExtendOwner(InterfaceDeclaration owner, CategoryDeclaration category)
        {
            Debug.Assert(owner == category.ExtendedInterface);

            foreach (MethodDeclaration method in category.Methods)
            {
                owner.Methods.Add(method);
            }
            foreach (PropertyDeclaration property in category.Properties)
            {
                owner.Properties.Add(property);
            }
            foreach (ProtocolDeclaration protocol in category.ImplementedProtocols)
            {
                owner.ImplementedProtocols.Add(protocol);
            }
        }
    }
}

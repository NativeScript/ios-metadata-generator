using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Generator;
using MetadataGenerator.Core.Meta.Filters;
using MetadataGenerator.Core.Meta.Utils;
using MetadataGenerator.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGenerator.Core.Meta.Visitors
{
    internal class TransformationVisitor : IMetaContainerDeclarationVisitor
    {
        public TransformationVisitor(ModuleDeclarationsContainer container, params IDeclarationVisitor[] visitors)
        {
            this.container = container;
            this.visitors = visitors;
            this.methodsToRemove = new List<MethodDeclaration>();
            this.propertiesToRemove = new List<PropertyDeclaration>();
            this.typesCache = new Dictionary<TypeDefinition, bool>();
            this.declarationsCache = new Dictionary<BaseDeclaration, bool>();
        }

        private readonly ModuleDeclarationsContainer container;
        private readonly List<MethodDeclaration> methodsToRemove;
        private readonly List<PropertyDeclaration> propertiesToRemove;
        private readonly Dictionary<TypeDefinition, bool> typesCache;
        private readonly Dictionary<BaseDeclaration, bool> declarationsCache;
        private readonly IDeclarationVisitor[] visitors;

        public static IEnumerable<ModuleDeclarationsContainer> Transform(IEnumerable<ModuleDeclaration> modules, params IDeclarationVisitor[] visitors)
        {
            foreach (IDeclarationVisitor visitor in visitors)
            {
                new VisitorFilter(visitor).Filter(modules);
            }

            List<ModuleDeclarationsContainer> containers = new List<ModuleDeclarationsContainer>();
            var toplevelModules = modules.Where(m => m.Parent == null);
            foreach (ModuleDeclaration module in toplevelModules)
            {
                ModuleDeclarationsContainer container = new ModuleDeclarationsContainer(module.Name);
                TransformationVisitor visitor = new TransformationVisitor(container, visitors);

                visitor.Begin(container);
                module.Accept(visitor);
                visitor.End(container);

                if (container.Count > 0)
                    containers.Add(container);
            }
            return containers;
        }

        public void Begin(ModuleDeclarationsContainer metaContainer)
        {
        }

        public void End(ModuleDeclarationsContainer metaContainer)
        {
            foreach (MethodDeclaration method in methodsToRemove)
            {
                BaseClass owner = method.Parent;
                owner.Methods.Remove(method);
            }
            foreach (PropertyDeclaration property in propertiesToRemove)
            {
                BaseClass owner = property.Parent;
                owner.Properties.Remove(property);
            }
        }

        private bool IsSupported(BaseDeclaration declaration)
        {
            return declaration.IsSupported(this.typesCache, this.declarationsCache);
        }

        public void Visit(InterfaceDeclaration declaration)
        {
            if (!IsSupported(declaration))
                return;

            container.Add(declaration);
        }

        public void Visit(ProtocolDeclaration declaration)
        {
            if (!IsSupported(declaration))
                return;

            container.Add(declaration);
        }

        public void Visit(CategoryDeclaration declaration)
        {
            if (!IsSupported(declaration) || !IsSupported(declaration.ExtendedInterface))
                return;

            container.Add(declaration);
        }

        public void Visit(StructDeclaration declaration)
        {
            if (!IsSupported(declaration))
                return;

            container.Add(declaration);
        }

        public void Visit(UnionDeclaration declaration)
        {
        }

        public void Visit(FieldDeclaration declaration)
        {
        }

        public void Visit(EnumDeclaration declaration)
        {
            if (!IsSupported(declaration) || declaration.IsAnonymousWithoutTypedef())
                return;
            
            container.Add(declaration);
        }

        public void Visit(EnumMemberDeclaration declaration)
        {
            if (!IsSupported(declaration))
                return;

            if (declaration.Parent.IsAnonymousWithoutTypedef())
            {
                var newMeta = new VarDeclaration(declaration.Name, declaration.Parent.UnderlyingType)
                {
                    Module = declaration.Parent.Module,
                    Location = declaration.Location,
                    IosAvailability = declaration.IosAvailability
                };
                newMeta.SetValue(declaration.Value);

                foreach (IDeclarationVisitor visitor in this.visitors)
                {
                    visitor.Visit(newMeta);
                }

                container.Add(newMeta);
            }
        }

        public void Visit(FunctionDeclaration declaration)
        {
            // TODO: Should we filter those "not valid" functions?
            if (!IsSupported(declaration) || !declaration.IsValidFunction())
                return;

            container.Add(declaration);
        }

        public void Visit(MethodDeclaration declaration)
        {
            if (declaration.IsImplicit || !IsSupported(declaration) ||
                declaration.Parent.Properties.Any(p => p.Getter == declaration || p.Setter == declaration))
            {
                // remove this method
                methodsToRemove.Add(declaration);
            }
        }

        public void Visit(ParameterDeclaration declaration)
        {
        }

        public void Visit(PropertyDeclaration declaration)
        {
            if (!IsSupported(declaration))
            {
                // remove this property
                propertiesToRemove.Add(declaration);
            }
        }

        public void Visit(ModuleDeclaration declaration)
        {
        }

        public void Visit(VarDeclaration declaration)
        {
            if (!IsSupported(declaration))
                return;

            container.Add(declaration);
        }

        public void Visit(TypedefDeclaration declaration)
        {
        }
    }
}

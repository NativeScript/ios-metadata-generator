using Libclang.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libclang.Core.Ast.Filters
{
    class RemoveNotSupportedDeclarationsFilters : IDeclarationsFilter
    {
        public IEnumerable<BaseDeclaration> Filter(IEnumerable<BaseDeclaration> declarations)
        {
            Dictionary<TypeDefinition, bool> typesCache = new Dictionary<TypeDefinition, bool>();
            Dictionary<BaseDeclaration, bool> declarationsCache = new Dictionary<BaseDeclaration, bool>();

            // Remove not supported declarations
            declarations = declarations.Where(d => d.IsSupported(typesCache, declarationsCache)).ToArray();

            IEnumerable<ProtocolDeclaration> protocols = declarations.OfType<ProtocolDeclaration>();
            IEnumerable<InterfaceDeclaration> interfaces = declarations.OfType<InterfaceDeclaration>();
            IEnumerable<CategoryDeclaration> categories = interfaces.SelectMany(i => i.Categories);

            IEnumerable<BaseClass> classes = protocols.Union(interfaces.Cast<BaseClass>()).Union(categories.Cast<BaseClass>());

            // Remove not supported methods and properties from base classes
            foreach (BaseClass baseClass in classes)
            {
                RemoveNotSupportedFromBaseClass(typesCache, declarationsCache, baseClass);
            }

            return declarations;
        }

        private static void RemoveNotSupportedFromBaseClass(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache, BaseClass protocol)
        {
            IEnumerable<MethodDeclaration> methodsToRemove = protocol.Methods.Where(m => !m.IsSupported(typesCache, declarationsCache)).ToArray();
            IEnumerable<PropertyDeclaration> propertiesToRemove = protocol.Properties.Where(p => !p.IsSupported(typesCache, declarationsCache)).ToArray();

            foreach (MethodDeclaration method in methodsToRemove)
            {
                protocol.Methods.Remove(method);
            }
            foreach (PropertyDeclaration property in propertiesToRemove)
            {
                protocol.Properties.Remove(property);
            }
        }
    }
}

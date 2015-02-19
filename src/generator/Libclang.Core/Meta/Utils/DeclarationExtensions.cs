using Libclang.Core.Ast;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Meta.Utils
{
    internal static class DeclarationExtensions
    {
        public static IEnumerable<MethodDeclaration> AllMethods(this ProtocolDeclaration protocol)
        {
            return protocol == null ? Enumerable.Empty<MethodDeclaration>() :
                protocol.Methods.Concat(protocol.ImplementedProtocols.SelectMany(AllMethods));
        }

        public static IEnumerable<MethodDeclaration> AllMethods(this CategoryDeclaration category)
        {
            return category.Methods.Concat(category.ImplementedProtocols.SelectMany(AllMethods));
        }

        public static IEnumerable<MethodDeclaration> AllMethods(this InterfaceDeclaration iface)
        {
            return iface == null ? Enumerable.Empty<MethodDeclaration>() :
                iface.Methods
                // NOTE: Here we may iterate the same protocol multiple times...
                .Concat(iface.Categories.SelectMany(AllMethods))
                .Concat(iface.ImplementedProtocols.SelectMany(AllMethods))
                .Concat(iface.Base.AllMethods());
        }
    }
}

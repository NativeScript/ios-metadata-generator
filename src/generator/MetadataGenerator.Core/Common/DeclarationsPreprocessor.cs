using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Types;
using Libclang.Core.Ast.Filters;

namespace Libclang.Core.Common
{
    public class DeclarationsPreprocessor
    {
        public IEnumerable<ModuleDeclaration> Process(IEnumerable<ModuleDeclaration> documents)
        {
            FixParameterNameCollisions(documents);
            RemoveAnonymousMembers(documents);
            ApplyDeclarationsFilter(new RemoveNotSupportedDeclarationsFilters(), documents);

            return documents;
        }

        private static void FixParameterNameCollisions(IEnumerable<ModuleDeclaration> documents)
        {
            foreach (ModuleDeclaration document in documents)
            {
                foreach (InterfaceDeclaration @interface in document.Interfaces)
                {
                    foreach (MethodDeclaration method in @interface.Methods)
                    {
                        FixParameterNameCollision(method);
                    }
                }

                foreach (ProtocolDeclaration protocol in document.Protocols)
                {
                    foreach (MethodDeclaration method in protocol.Methods)
                    {
                        FixParameterNameCollision(method);
                    }
                }
            }
        }

        private static void RemoveAnonymousMembers(IEnumerable<ModuleDeclaration> documents)
        {
            foreach (ModuleDeclaration document in documents)
            {
                var anonymousRecords = document.Declarations
                    .OfType<BaseRecordDeclaration>()
                    .Where(c => c.IsAnonymousWithoutTypedef())
                    .ToArray();

                foreach (IDeclaration @record in anonymousRecords)
                {
                    document.Declarations.Remove(@record);
                }
            }
        }

        private static void FixParameterNameCollision(MethodDeclaration method)
        {
            if (method.Parameters.Count == 2 && method.Parameters[0].Name == method.Parameters[1].Name)
            {
                method.Parameters[1].Name += 1;
            }
        }

        private static void ApplyDeclarationsFilter(IDeclarationsFilter filter, IEnumerable<ModuleDeclaration> documents)
        {
            foreach (ModuleDeclaration document in documents)
            {
                filter.Filter(document.Declarations);
            }
        }
    }
}

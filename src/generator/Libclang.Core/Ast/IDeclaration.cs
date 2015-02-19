using System;
using Libclang.Core.Common;
using Libclang.Core.Types;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    public interface IDeclaration
    {
        string Name { get; }

        string FullName { get; }

        IEnumerable<TypeDefinition> ReferedTypes { get; } 

        Location Location { get; }

        IDeclaration Canonical { get; }

        void Accept(Meta.Visitors.IDeclarationVisitor visitor);
    }
}

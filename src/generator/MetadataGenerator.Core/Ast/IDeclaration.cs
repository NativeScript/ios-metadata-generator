using System;
using MetadataGenerator.Core.Common;
using MetadataGenerator.Core.Types;
using System.Collections.Generic;

namespace MetadataGenerator.Core.Ast
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

using System.Collections.Generic;
using MetadataGenerator.Core.Types;

namespace MetadataGenerator.Core.Ast
{
    public interface IFunction
    {
        TypeDefinition ReturnType { get; set; }

        IList<ParameterDeclaration> Parameters { get; }

        bool IsVariadic { get; set; }

        IEnumerable<TypeDefinition> ReferedTypes { get; }
    }
}

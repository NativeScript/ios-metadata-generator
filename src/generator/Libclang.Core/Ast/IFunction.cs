using System.Collections.Generic;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public interface IFunction
    {
        TypeDefinition ReturnType { get; set; }

        IList<ParameterDeclaration> Parameters { get; }

        bool IsVariadic { get; set; }

        IEnumerable<TypeDefinition> ReferedTypes { get; }
    }
}

using System;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    internal interface IProtocolImplementer
    {
        ICollection<ProtocolDeclaration> ImplementedProtocols { get; }
    }
}

using System;
using System.Collections.Generic;

namespace MetadataGenerator.Core.Ast
{
    internal interface IProtocolImplementer
    {
        ICollection<ProtocolDeclaration> ImplementedProtocols { get; }
    }
}

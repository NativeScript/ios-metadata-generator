using System;
using System.Linq;

namespace Libclang.Core.Meta.Utils
{
    public enum MetaStructureType
    {
        Undefined = 0,
        Struct,
        Union,
        Function,
        JsCode,
        Var,
        Interface,
        Protocol
    }
}

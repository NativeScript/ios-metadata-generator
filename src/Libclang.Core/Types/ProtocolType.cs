using Libclang.Core.Ast;
using System;

namespace Libclang.Core.Types
{
    public class ProtocolType : TypeDefinition
    {
        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + "Protocol" + identifier;
        }

        public override TypeEncoding ToTypeEncoding(Func<BaseDeclaration, string> jsNameCalculator)
        {
            return TypeEncoding.Protocol;
        }
    }
}

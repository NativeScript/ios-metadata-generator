using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Ast;

namespace Libclang.Core.Types
{
    public class IdType : TypeDefinition, IProtocolImplementer
    {
        public ICollection<ProtocolDeclaration> ImplementedProtocols { get; private set; }

        public IdType()
        {
            this.ImplementedProtocols = new List<ProtocolDeclaration>();
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + "id" +
                   (ImplementedProtocols.Any()
                       ? string.Format("<{0}>", string.Join(", ", ImplementedProtocols.Select(x => x.Name)))
                       : "") + identifier;
        }

        public override TypeEncoding ToTypeEncoding(Func<BaseDeclaration, string> jsNameCalculator)
        {
            return TypeEncoding.Id(this.ImplementedProtocols.Select(jsNameCalculator));
        }
    }
}

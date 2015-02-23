using System;
using System.Linq;
using System.Collections.Generic;
using MetadataGenerator.Core.Ast;

namespace MetadataGenerator.Core.Types
{
    public class ClassType : TypeDefinition, IProtocolImplementer
    {
        public ICollection<ProtocolDeclaration> ImplementedProtocols { get; private set; }

        public ClassType()
        {
            this.ImplementedProtocols = new List<ProtocolDeclaration>();
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + "Class" +
                   (ImplementedProtocols.Any()
                       ? string.Format("<{0}>", string.Join(", ", ImplementedProtocols.Select(x => x.Name)))
                       : "") + identifier;
        }

        public override TypeEncoding ToTypeEncoding()
        {
            return TypeEncoding.Class;
        }
    }
}

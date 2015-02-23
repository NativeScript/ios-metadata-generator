using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Meta.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGenerator.Core.Types
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

        public override TypeEncoding ToTypeEncoding()
        {
            Func<ProtocolDeclaration, Tuple<string, string>> action = p =>
            {
                string module = p.Module != null ? p.Module.FullName : "";
                string jsName = p.GetJSName();
                if (string.IsNullOrEmpty(jsName))
                {
                    jsName = p.Name;
                }
                return new Tuple<string, string>(module, jsName);
            };

            return TypeEncoding.Id(this.ImplementedProtocols.Select(action));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Types
{
    public class ComplexType : TypeDefinition
    {
        public TypeDefinition Type { get; set; }

        public ComplexType(TypeDefinition type)
        {
            this.Type = type;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.Type });
            }
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + string.Format("complex{0}", this.Type.ToStringInternal(identifier));
        }
    }
}

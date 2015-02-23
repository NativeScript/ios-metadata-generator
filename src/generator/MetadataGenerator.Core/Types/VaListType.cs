using Libclang.Core.Ast;
using System;
using System.Collections.Generic;

namespace Libclang.Core.Types
{
    public class VaListType : TypeDefinition
    {
        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + "va_list" + identifier;
        }

        protected override bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
 	         return false;
        }

        public override TypeEncoding ToTypeEncoding()
        {
            return TypeEncoding.VaList;
        }
    }
}

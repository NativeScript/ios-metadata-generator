using Libclang.Core.Ast;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Types
{
    public class IncompleteArrayType : TypeDefinition
    {
        public TypeDefinition ElementType { get; set; }

        public IncompleteArrayType(TypeDefinition elementType)
        {
            this.ElementType = elementType;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.ElementType });
            }
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            string format = isOuter ? "{0}[]" : "({0})[]";
            return ToStringHelper() + this.ElementType.ToStringInternal(string.Format(format, identifier));
        }

        public override TypeEncoding ToTypeEncoding(Func<BaseDeclaration, string> jsNameCalculator)
        {
            return TypeEncoding.IncompleteArray(this.ElementType.ToTypeEncoding(jsNameCalculator));
        }
    }
}

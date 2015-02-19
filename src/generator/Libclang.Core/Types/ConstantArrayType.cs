using Libclang.Core.Ast;
using System;
using System.Linq;

namespace Libclang.Core.Types
{
    public class ConstantArrayType : IncompleteArrayType
    {
        public int Size { get; set; }

        public ConstantArrayType(int size, TypeDefinition elementType)
            : base(elementType)
        {
            this.Size = size;
        }

        public override TypeEncoding ToTypeEncoding()
        {
            return TypeEncoding.ConstantArray(this.Size, this.ElementType.ToTypeEncoding());
        }

#if DEBUG
        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            string format = isOuter ? "{0}[{1}]" : "({0})[{1}]";

            return ToStringHelper() + this.ElementType.ToStringInternal(string.Format(format, identifier, this.Size));
        }
#endif
    }
}

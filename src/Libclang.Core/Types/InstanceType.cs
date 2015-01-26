using System;
using System.Collections.Generic;
using Libclang.Core.Ast;

namespace Libclang.Core.Types
{
    public class InstanceType : TypeDefinition
    {
        public static TypeDefinition ToReference(InterfaceDeclaration declaration)
        {
            return new PointerType(new DeclarationReferenceType(declaration));
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            if (identifier.Length > 0)
            {
                identifier = " " + identifier;
            }
            return ToStringHelper() + "instancetype" + identifier;
        }

        public override TypeEncoding ToTypeEncoding(Func<BaseDeclaration, string> jsNameCalculator)
        {
            return TypeEncoding.Instancetype;
        }
    }
}

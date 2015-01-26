using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Ast;

namespace Libclang.Core.Types
{
    public class PointerType : TypeDefinition
    {
        public TypeDefinition Target { get; set; }

        public PointerType(TypeDefinition target)
        {
            this.Target = target;
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            return ToStringHelper() + this.Target.ToStringInternal("*" + identifier);
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.Target });
            }
        }

        public override TypeEncoding ToTypeEncoding(Func<BaseDeclaration, string> jsNameCalculator)
        {
            
            TypeDefinition type = this.Target.Resolve();
            DeclarationReferenceType dclrType = type as DeclarationReferenceType;
            PrimitiveType primitiveType = type as PrimitiveType;

            // if is pointer to interface e.g. NSArray *
            if (dclrType != null && dclrType.Target is InterfaceDeclaration)
            {
                string interfaceName = jsNameCalculator(dclrType.Target);
                return TypeEncoding.Interface(interfaceName);
            }
            else if (primitiveType != null && (primitiveType.Type == PrimitiveTypeType.CharS || primitiveType.Type == PrimitiveTypeType.UChar))
            {
                return TypeEncoding.CString;
            }
            else if (this.Target.Resolve() is ProtocolType)
            {
                return TypeEncoding.Protocol;
            }

            return TypeEncoding.Pointer(this.Target.ToTypeEncoding(jsNameCalculator));
        }
    }
}

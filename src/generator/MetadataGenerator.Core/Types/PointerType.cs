using System;
using System.Collections.Generic;
using System.Linq;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Generator;
using MetadataGenerator.Core.Meta.Utils;

namespace MetadataGenerator.Core.Types
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

        public override TypeEncoding ToTypeEncoding()
        {
            TypeDefinition type = this.Target.Resolve();
            DeclarationReferenceType dclrType = type as DeclarationReferenceType;
            PrimitiveType primitiveType = type as PrimitiveType;

            // if is pointer to interface e.g. NSArray *
            if (dclrType != null && dclrType.Target is InterfaceDeclaration)
            {
                if (dclrType.Target.Module != null)
                {
                    return TypeEncoding.Interface(dclrType.Target.GetJSName(), dclrType.Target.Module.FullName);
                }
                return TypeEncoding.Unknown;
            }
            else if (primitiveType != null && (primitiveType.Type == PrimitiveTypeType.CharS || primitiveType.Type == PrimitiveTypeType.UChar))
            {
                return TypeEncoding.CString;
            }
            else if (this.Target.Resolve() is ProtocolType)
            {
                return TypeEncoding.Protocol;
            }

            return TypeEncoding.Pointer(this.Target.ToTypeEncoding());
        }
    }
}

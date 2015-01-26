using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public class TypedefDeclaration : BaseDeclaration
    {
        public TypeDefinition OldType { get; set; }

        public TypeDefinition UnderlyingType
        {
            get
            {
                var result = this.OldType;

                while ((result is DeclarationReferenceType) &&
                       (result as DeclarationReferenceType).Target is TypedefDeclaration)
                {
                    result = ((result as DeclarationReferenceType).Target as TypedefDeclaration).OldType;
                }

                return result;
            }
        }

        public TypedefDeclaration(string newName, TypeDefinition old)
            : base(newName)
        {
            this.OldType = old;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.OldType });
            }
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("TYPEDEF_DECLARATION: {0} -> {1}", this.Name, this.OldType);
        }
#endif
    }
}

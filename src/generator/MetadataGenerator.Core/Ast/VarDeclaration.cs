using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public class VarDeclaration : BaseDeclaration
    {
        public TypeDefinition Type { get; set; }

        public VarDeclaration(string name, TypeDefinition type)
            : base(name)
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

#if DEBUG
        public override string ToString()
        {
            return string.Format("VAR_DECLARATION: {0} - {1}", this.Type, this.Name);
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

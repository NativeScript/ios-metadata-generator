using Libclang.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Ast
{
    public class EnumMemberDeclaration : BaseDeclaration
    {
        public decimal Value { get; set; }

        public EnumDeclaration Parent { get; set; }

        public EnumMemberDeclaration(string name, decimal value, EnumDeclaration parent)
            : base(name)
        {
            this.Value = value;
            this.Parent = parent;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.Parent.UnderlyingType });
            }
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("{0} = {1}", this.Name, this.Value);
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

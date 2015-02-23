using System;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    public class UnionDeclaration : BaseRecordDeclaration
    {
        public override string FullName
        {
            get { return "union " + base.FullName; }
        }

        public UnionDeclaration(string name)
            : base(name)
        {
        }

        protected override bool? IsSupportedInternal(Dictionary<Types.TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            return false;
        }

#if DEBUG
        public override string ToString()
        {
            return "UNION_DECLARATION: " + ToStringHelper();
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            base.Accept(visitor);
            visitor.Visit(this);
        }
    }
}

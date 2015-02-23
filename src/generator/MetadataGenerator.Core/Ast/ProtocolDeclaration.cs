using System;
using System.Linq;

namespace Libclang.Core.Ast
{
    public class ProtocolDeclaration : BaseClass
    {
        public override string FullName
        {
            get { return "protocol " + base.FullName; }
        }

        public ProtocolDeclaration(string name)
            : base(name)
        {
        }

#if DEBUG
        public override string ToString()
        {
            return "PROTOCOL_DECLARATION: " + this.Name +
                   ToStringHelper();
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            base.Accept(visitor);
            visitor.Visit(this);
        }
    }
}

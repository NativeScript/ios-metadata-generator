using System;

namespace Libclang.Core.Ast
{
    public class StructDeclaration : BaseRecordDeclaration
    {
        public override string FullName
        {
            get { return "struct " + base.FullName; }
        }

        public StructDeclaration(string name)
            : base(name)
        {
        }

#if DEBUG
        public override string ToString()
        {
            return "STRUCT_DECLARATION: " + ToStringHelper();
        }
#endif
    }
}

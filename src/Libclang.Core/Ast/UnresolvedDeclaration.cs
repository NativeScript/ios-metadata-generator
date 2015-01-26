using System;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    public class UnresolvedDeclaration : BaseDeclaration
    {
        public UnresolvedDeclaration(string name)
            : base(name)
        {
        }

        protected override bool? IsSupportedInternal(Dictionary<Types.TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            return false;
        }
    }
}

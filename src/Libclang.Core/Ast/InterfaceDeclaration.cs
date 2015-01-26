using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Common;
using Libclang.Core.Types;

namespace Libclang.Core.Ast
{
    public class InterfaceDeclaration : BaseClass
    {
        public InterfaceDeclaration Base { get; set; }

        public ICollection<CategoryDeclaration> Categories { get; private set; }

        public InterfaceDeclaration(string name)
            : base(name)
        {
            this.Categories = new List<CategoryDeclaration>();
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                var result = base.ReferedTypes;
                result = (this.Base != null) ? result.Union(this.Base.ReferedTypes) : result;
                return result;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return "INTERFACE_DECLARATION: " + this.Name +
                   (this.Base != null ? " : " + this.Base.Name : "") +
                   ToStringHelper();
        }
#endif
    }
}

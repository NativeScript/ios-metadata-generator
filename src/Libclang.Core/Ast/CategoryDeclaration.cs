using Libclang.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Ast
{
    public class CategoryDeclaration : BaseClass
    {
        public override string FullName
        {
            get { return string.Format("{0}@{1}", this.ExtendedInterface.Name, this.Name); }
        }

        public InterfaceDeclaration ExtendedInterface { get; set; }

        public bool IsClassExtension
        {
            get { return this.Name.Length == 0; }
        }

        public CategoryDeclaration(string name, InterfaceDeclaration extendedInterface)
            : base(name)
        {
            this.ExtendedInterface = extendedInterface;
        }

#if DEBUG
        public override string ToString()
        {
            return "CATEGORY_DECLARATION: " + string.Format("{0} ({1})", this.ExtendedInterface.Name, this.Name) +
                   ToStringHelper();
        }
#endif
    }
}

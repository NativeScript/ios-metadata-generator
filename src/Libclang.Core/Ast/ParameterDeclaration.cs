using System;
using System.Linq;
using System.Text;
using Libclang.Core.Types;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    public class ParameterDeclaration : BaseDeclaration
    {
        public TypeDefinition Type { get; set; }

        public bool IsIn { get; set; }
        public bool IsInout { get; set; }
        public bool IsOut { get; set; }
        public bool IsBycopy { get; set; }
        public bool IsByref { get; set; }
        public bool IsOneway { get; set; }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return base.ReferedTypes.Union(new List<TypeDefinition>() { this.Type });
            }
        }

        public ParameterDeclaration(string name, TypeDefinition type)
            : base(name)
        {
            this.Name = name;
            this.Type = type;
        }

#if DEBUG
        public override string ToString()
        {
            var annotations = new StringBuilder();

            if (this.IsIn) annotations.Append("in ");
            if (this.IsInout) annotations.Append("inout ");
            if (this.IsOut) annotations.Append("out ");
            if (this.IsBycopy) annotations.Append("bycopy ");
            if (this.IsByref) annotations.Append("byref ");
            if (this.IsOneway) annotations.Append("oneway ");

            return annotations.ToString() + this.Type + (this.Name != null ? " " + this.Name : "");
        }
#endif
    }
}

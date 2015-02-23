using System;
using System.Linq;
using MetadataGenerator.Core.Types;

namespace MetadataGenerator.Core.Ast
{
    public class BitFieldDeclaration : FieldDeclaration
    {
        public int Width { get; set; }

        public BitFieldDeclaration(string name, TypeDefinition type, int width)
            : base(name, type)
        {
            this.Width = width;
        }

#if DEBUG
        public override string ToString()
        {
            return string.Format("{0}: {1} bits", this.Name, this.Width);
        }
#endif
    }
}

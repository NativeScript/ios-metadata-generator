using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGenerator.Core.Types
{
    public class VectorType : TypeDefinition
    {
        public int Size { get; set; }

        public TypeDefinition ElementType { get; set; }

        public VectorType(int size, TypeDefinition elementType)
        {
            this.Size = size;
            this.ElementType = elementType;
        }

        public override IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                var result = base.ReferedTypes.ToList();
                if (this.ElementType != null)
	            {
                    result.Add(this.ElementType);
	            }
                return result;
            }
        }

        protected override bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<Ast.BaseDeclaration, bool> declarationsCache)
        {
            return false;
        }

        internal override string ToStringInternal(string identifier, bool isOuter = false)
        {
            return string.Format("__vector {0} {1}", ElementType.ToStringInternal(identifier, isOuter), identifier);
        }
    }
}

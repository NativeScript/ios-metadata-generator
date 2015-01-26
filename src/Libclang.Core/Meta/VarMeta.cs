using System;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class VarMeta : Meta
    {
        public object Value { get; set; }

        public TypeEncoding ExtendedEncoding { get; set; }

        public bool HasValue
        {
            get
            {
                return this.Value != null;
            }
        }

        public bool TryGetValue(out object value)
        {
            value = this.Value;
            return this.HasValue;
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            if (this.HasValue)
            {
                return structure.ChangeToJsCode(this.Value.ToString());
            }
            structure.Type = MetaStructureType.Var;
            structure.Info = new Pointer(this.ExtendedEncoding.ToString());
            return structure;
        }
    }
}

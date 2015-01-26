using System;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class StructMeta : RecordMeta
    {
        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            structure.Type = MetaStructureType.Struct;
            return structure;
        }
    }
}

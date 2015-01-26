using Libclang.Core.Meta.Utils;
using System;
using System.Linq;

namespace Libclang.Core.Meta
{
    public class UnionMeta : RecordMeta
    {
        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            structure.Type = MetaStructureType.Union;
            return structure;
        }
    }
}

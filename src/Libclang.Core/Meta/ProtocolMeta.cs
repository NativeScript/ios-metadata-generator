using System;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class ProtocolMeta : BaseClassMeta
    {
        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            structure.Type = MetaStructureType.Protocol;
            return structure;
        }
    }
}

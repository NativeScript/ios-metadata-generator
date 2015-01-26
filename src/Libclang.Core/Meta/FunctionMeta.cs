using System;
using System.Linq;
using Libclang.Core.Meta.Utils;
using System.Collections.Generic;

namespace Libclang.Core.Meta
{
    public class FunctionMeta : Meta
    {
        public FunctionMeta()
        {
            this.Parameters = new List<ParameterMeta>();
        }

        public bool IsVariadic { get; set; }

        public bool HasVaListParameter { get; set; }

        public bool IsDefinedInHeaders { get; set; }

        public bool OwnsReturnedCocoaObject { get; set; }

        public TypeEncoding ExtendedEncoding
        {
            get
            {
                return TypeEncoding.Call(this.ReturnTypeEncoding, this.Parameters.Select(p => p.TypeEncoding));
            }
        }

        public IList<ParameterMeta> Parameters { get; private set; }

        public TypeEncoding ReturnTypeEncoding { get; set; }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();
            structure.Type = MetaStructureType.Function;
            structure.Info = new Pointer(this.ExtendedEncoding.ToString());

            structure.Flags[BinaryMetaStructure.FunctionIsVariadic] = this.IsVariadic;
            structure.Flags[BinaryMetaStructure.FunctionOwnsReturnedCocoaObject] = this.OwnsReturnedCocoaObject;
            return structure;
        }
    }
}

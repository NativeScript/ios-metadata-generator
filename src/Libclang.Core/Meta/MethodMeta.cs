using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class MethodMeta : MemberMeta
    {
        public MethodMeta()
        {
            this.Parameters = new List<ParameterMeta>();
        }

        public bool IsVariadic { get; set; }

        public bool IsNilTerminatedVariadic { get; set; }

        public bool IsStatic { get; set; }

        public string Selector { get; set; }

        public bool HasVaListParameter { get; set; }

        public bool OwnsReturnedCocoaObject { get; set; }

        public override TypeEncoding ExtendedEncoding
        {
            get
            {
                return TypeEncoding.Call(this.ReturnTypeEncoding, this.Parameters.Select(p => p.TypeEncoding));
            }
        }

        public string CompilerEncoding { get; set; }

        public bool IsConstructor { get; set; }

        public bool IsOptional { get; set; }

        public IList<ParameterMeta> Parameters { get; private set; }

        public TypeEncoding ReturnTypeEncoding { get; set; }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();

            structure.Info = new List<object>()
            {
                new Pointer(this.Selector),
                new Pointer(this.ExtendedEncoding.ToString()),
                new Pointer(this.CompilerEncoding)
            };

            // set flags
            structure.Flags[BinaryMetaStructure.MethodIsVariadic] = this.IsVariadic;
            structure.Flags[BinaryMetaStructure.MethodIsNullTerminatedVariadic] = this.IsNilTerminatedVariadic;
            structure.Flags[BinaryMetaStructure.MethodOwnsReturnedCocoaObject] = this.OwnsReturnedCocoaObject;

            return structure;
        }
    }
}

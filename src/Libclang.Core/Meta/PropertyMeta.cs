using Libclang.Core.Types;
using System;
using System.Linq;
using System.Collections.Generic;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class PropertyMeta : MemberMeta
    {
        public MethodMeta Getter { get; set; }

        public MethodMeta Setter { get; set; }

        public bool IsOptional { get; set; }

        public override TypeEncoding ExtendedEncoding
        {
            get
            {
                if (this.Getter != null)
                {
                    return this.Getter.ReturnTypeEncoding;
                }
                else
                {
                    if (!this.Setter.ReturnTypeEncoding.IsVoid())
                    {
                        throw new Exception("Invalid property setter. The setter should return void.");
                    }

                    if (this.Setter.Parameters.Count() != 1)
                    {
                        throw new Exception("Invalid property setter. The setter should have only one parameter.");
                    }

                    return this.Setter.Parameters.First().TypeEncoding;
                }
            }
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();

            List<object> propertyInfo = new List<object>();
            if (this.Getter != null)
            {
                propertyInfo.Add(new Pointer(this.Getter.GetBinaryStructure()));
            }
            if (this.Setter != null)
            {
                propertyInfo.Add(new Pointer(this.Setter.GetBinaryStructure()));
            }

            structure.Info = propertyInfo;

            // set flags
            structure.Flags[BinaryMetaStructure.PropertyHasGetter] = (this.Getter != null);
            structure.Flags[BinaryMetaStructure.PropertyHasSetter] = (this.Setter != null);

            return structure;
        }
    }
}

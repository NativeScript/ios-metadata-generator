using Libclang.Core.Meta.Utils;
using System;
using System.Linq;

namespace Libclang.Core.Meta
{
    public abstract class MemberMeta : Meta
    {
        public string ParentJsName { get; set; }

        public bool? IsLocalJsNameDuplicate { get; set; }

        public bool? HasJsNameDuplicateInHierarchy { get; set; }

        public abstract TypeEncoding ExtendedEncoding { get; }

        public BaseClassMeta Parent
        {
            get { return (this.ParentJsName == null) ? null : (BaseClassMeta) this.Container[this.ParentJsName]; }
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();

            // remove the real name if is set
            structure.Name = structure.JsName;
            structure.Flags[BinaryMetaStructure.HasName] = false;

            // set flags
            bool isLocalJsNameDuplicate = this.IsLocalJsNameDuplicate.HasValue && this.IsLocalJsNameDuplicate.Value;
            bool hasJsNameDuplicateInHierarchy = this.HasJsNameDuplicateInHierarchy.HasValue && this.HasJsNameDuplicateInHierarchy.Value;

            structure.Flags[BinaryMetaStructure.MemberIsLocalJsNameDuplicate] = isLocalJsNameDuplicate;
            structure.Flags[BinaryMetaStructure.MemberHasJsNameDuplicateInHierarchy] = hasJsNameDuplicateInHierarchy;

            return structure;
        }
    }
}

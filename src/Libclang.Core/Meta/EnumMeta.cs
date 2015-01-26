using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Types;

namespace Libclang.Core.Meta
{
    public class EnumMeta : Meta
    {
        public string TypedefName { get; set; }

        public bool IsAnonymous { get; set; }

        public IList<EnumFieldMeta> Fields { get; set; }

        public TypeDefinition UnderlyingType { get; set; }

        public bool IsAnonymousWithoutTypedef()
        {
            return this.IsAnonymous && string.IsNullOrEmpty(this.TypedefName);
        }

        public bool IsAnonymousWithTypedef()
        {
            return this.IsAnonymous && !string.IsNullOrEmpty(this.TypedefName);
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();

            string json = String.Format("{{{0}}}", String.Join(",", this.Fields.Select(f => String.Format("\"{0}\":{1}", f.Name, f.Value))));
            string jsCode = String.Format("__tsEnum({0})", json);
            structure.ChangeToJsCode(jsCode);

            return structure;
        }
    }
}

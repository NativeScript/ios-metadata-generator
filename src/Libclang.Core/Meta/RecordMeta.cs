using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public abstract class RecordMeta : Meta
    {
        public string TypedefName { get; set; }

        public bool IsAnonymous { get; set; }

        public IList<RecordFieldMeta> Fields { get; set; }

        public string ExtendedEncoding
        {
            get { return String.Join(string.Empty, this.Fields.Select(f => f.ExtendedEncoding)); }
        }

        public bool IsAnonymousWithTypedef()
        {
            return this.IsAnonymous && !string.IsNullOrEmpty(this.TypedefName);
        }

        public bool IsAnonymousWithoutTypedef()
        {
            return this.IsAnonymous && string.IsNullOrEmpty(this.TypedefName);
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = base.GetBinaryStructure();

            Pointer fieldsEncoding = new Pointer(this.ExtendedEncoding.ToString());
            IEnumerable<Pointer> fieldsNames = this.Fields.Select(f => new Pointer(f.JSName));

            List<object> array = new List<object>();
            array.Add(fieldsEncoding);
            array.Add(new ArrayCount((uint) fieldsNames.Count()));
            array.AddRange(fieldsNames);

            structure.Info = array;
            return structure;
        }
    }
}

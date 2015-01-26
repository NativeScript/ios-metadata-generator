using System;
using System.IO;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
{
    internal class BinarySerializer : BaseMetaFilter
    {
        private MetaFile file;

        public BinarySerializer(string outputFilePath)
            : base(null)
        {
            this.OutputFilePath = outputFilePath;
            this.file = null;
        }

        public string OutputFilePath { get; private set; }

        protected override Action<MetaContainer, Meta, string> ActionForEach
        {
            get { return this.SerializeMeta; }
        }

        protected override void Begin(MetaContainer metaContainer)
        {
            this.file = new MetaFile((int) (metaContainer.Count*1.25));
        }

        protected override void End(MetaContainer metaContainer)
        {
            this.file.SaveAs(this.OutputFilePath);
        }

        private void SerializeMeta(MetaContainer metaContainer, Meta meta, string key)
        {
            this.file.GlobalTable.AddMeta(meta.JSName, meta.GetBinaryStructure());
        }
    }
}

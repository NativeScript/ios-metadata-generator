using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataGenerator.DocsetParser
{
    public class TokenMetadata
    {
        public string Name { get; set; }

        public Type Type { get; set; }

        public string Abstract { get; set; }

        public string Anchor { get; set; }

        public string Declaration { get; set; }

        public string Module { get; set; }

        public string DeprecationSummary { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", this.Type.Name, this.Name, this.Abstract);
        }
    }
}

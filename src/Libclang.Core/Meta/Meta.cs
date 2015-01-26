using System;
using System.Linq;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Common;

namespace Libclang.Core.Meta
{
    public abstract class Meta
    {
        public MetaContainer Container { get; set; }

        public string Name { get; set; }

        public string JSName { get; set; }

        public string Framework { get; set; }

        public Common.Version IntroducedIn { get; set; }

        public Common.Version ObsoletedIn { get; set; }

        public Common.Version DeprecatedIn { get; set; }

        public bool IsIosAppExtensionAvailable { get; set; }

        public Location Location { get; set; }

        public virtual BinaryMetaStructure GetBinaryStructure()
        {
            BinaryMetaStructure structure = new BinaryMetaStructure();
            // Names
            structure.Name = this.Name;
            structure.JsName = this.JSName;

            // Framework
            structure.Framework = this.Framework;

            // Availability
            structure.IntrducedIn = this.IntroducedIn;
            structure.Flags[BinaryMetaStructure.IsIosAppExtensionAvailable] = this.IsIosAppExtensionAvailable;

            return structure;
        }
    }
}

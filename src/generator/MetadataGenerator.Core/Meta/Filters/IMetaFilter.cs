using System;
using System.Linq;
using MetadataGenerator.Core.Meta.Utils;

namespace MetadataGenerator.Core.Meta.Filters
{
    public interface IMetaFilter
    {
        void Filter(ModuleDeclarationsContainer metaContainer);
    }
}

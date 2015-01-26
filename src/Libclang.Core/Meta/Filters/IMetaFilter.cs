using System;
using System.Linq;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta.Filters
{
    public interface IMetaFilter
    {
        void Filter(MetaContainer metaContainer);
    }
}

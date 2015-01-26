using System;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Common;
using Libclang.Core.Meta.Utils;

namespace Libclang.Core.Meta
{
    public class InterfaceMeta : BaseClassMeta
    {
        public IEnumerable<CategoryMeta> Categories { get; set; }

        public string BaseJsName { get; set; }

        public IEnumerable<string> ImplementedProtocolsJsNamesWithCategories
        {
            get
            {
                return
                    this.ImplementedProtocolsJSNames.Union(this.Categories.SelectMany(c => c.ImplementedProtocolsJSNames))
                        .DistinctBy(n => n);
            }
        }

        public IEnumerable<ProtocolMeta> ImplementedProtocolsWithCategories
        {
            get { return this.ImplementedProtocolsJsNamesWithCategories.Select(n => (ProtocolMeta) this.Container[n]); }
        }

        public InterfaceMeta Base
        {
            get { return (this.BaseJsName == null) ? null : (InterfaceMeta) this.Container[this.BaseJsName]; }
        }

        public override BinaryMetaStructure GetBinaryStructure()
        {
            IEnumerable<MethodMeta> instanceMethods = this.InstanceMethods.Union(this.Categories.SelectMany(c => c.InstanceMethods));
            IEnumerable<MethodMeta> staticMethods = this.StaticMethods.Union(this.Categories.SelectMany(c => c.StaticMethods));
            IEnumerable<PropertyMeta> properties = this.Properties.Union(this.Categories.SelectMany(c => c.Properties));
            IEnumerable<string> protocols = this.ImplementedProtocolsJsNamesWithCategories;

            BinaryMetaStructure structure = this.Serialize(instanceMethods, staticMethods, properties, protocols);
            ((List<object>) structure.Info).Add(new Pointer(this.BaseJsName));
            structure.Type = MetaStructureType.Interface;
            return structure;
        }
    }
}

using System;
using System.Linq;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Types;
using System.IO;
using System.Collections.Generic;

namespace Libclang.Core.Meta.Filters
{
    class AnonymousEnumsToConstantsFilter : BaseMetaFilter
    {
        public AnonymousEnumsToConstantsFilter()
            : this(null)
        {
        }

        public AnonymousEnumsToConstantsFilter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<MetaContainer> ActionForContainer
        {
            get
            {
                return this.ConvertAnonymousEnum;
            }
        }

        public void ConvertAnonymousEnum(MetaContainer metaContainer)
        {
            IEnumerable<KeyValuePair<string, Meta>> anonymousEnums = metaContainer.Where(p => (p.Value is EnumMeta) && (p.Value as EnumMeta).IsAnonymousWithoutTypedef()).ToArray();
            
            foreach (KeyValuePair<string, Meta> pair in anonymousEnums)
            {
                string key = pair.Key;
                EnumMeta meta = (EnumMeta)pair.Value;

                this.Log("Enum: {0}:", meta.Name);
                metaContainer.Remove(key);

                foreach (EnumFieldMeta field in meta.Fields)
                {
                    this.Log("    - {0}", field.Name);
                    VarMeta constant = this.ToVarMeta(field, meta.UnderlyingType, metaContainer);
                    metaContainer.AddMeta(constant);
                }
                this.Log(String.Empty);
            }
        }

        private VarMeta ToVarMeta(EnumFieldMeta enumField, TypeDefinition underlyingType, MetaContainer metaContainer)
        {
            return new VarMeta()
            {
                Container = enumField.Container,
                Name = enumField.Name,
                JSName = enumField.JSName,
                Framework = enumField.Framework,
                IntroducedIn = enumField.IntroducedIn,
                ObsoletedIn = enumField.ObsoletedIn,
                DeprecatedIn = enumField.DeprecatedIn,
                ExtendedEncoding = underlyingType.ToTypeEncoding(metaContainer.CalculateJsName),
                Value = enumField.Value.ToString()
            };
        }
    }
}

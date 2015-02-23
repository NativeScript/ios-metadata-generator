using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetadataGenerator.Core.Ast;
using MetadataGenerator.Core.Common;

namespace MetadataGenerator.Core.Types
{
    public abstract class TypeDefinition
    {
        public bool IsConst { get; set; }

        public bool IsVolatile { get; set; }

        public bool IsRestrict { get; set; }

        protected string ToStringHelper()
        {
            var result = new StringBuilder();

            if (this.IsConst)
            {
                result.Append("const ");
            }
            if (this.IsVolatile)
            {
                result.Append("volatile ");
            }
            //if (this.IsRestrict)
            //{
            //	result.Append("restrict ");
            //}

            return result.ToString();
        }

        public override sealed string ToString()
        {
            return this.ToString(string.Empty);
        }

        public string ToString(string identifier)
        {
            return this.ToStringInternal(identifier, true);
        }

        internal abstract string ToStringInternal(string identifier, bool isOuter = false);

        public virtual IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return Enumerable.Empty<TypeDefinition>();
            }
        }

        public bool IsSupported(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if (typesCache.ContainsKey(this))
            {
                return typesCache[this];
            }
            typesCache.Add(this, true);

            bool isSupported = this.IsSupportedInternal(typesCache, declarationsCache) ?? this.RefersOnlySupportedTypes(typesCache, declarationsCache);
            typesCache[this] = isSupported;

            return isSupported;
        }

        protected virtual bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if (this.ToTypeEncoding().IsUnknown())
            {
                return false;
            }

            return null;
        }

        public bool RefersOnlySupportedTypes(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            foreach (TypeDefinition type in this.ReferedTypes.DistinctBy(t => t))
            {
                if (!type.IsSupported(typesCache, declarationsCache))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual TypeEncoding ToTypeEncoding()
        {
            return TypeEncoding.Unknown;
        }
    }
}

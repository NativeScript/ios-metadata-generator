using System;
using System.Linq;
using Libclang.Core.Common;
using Libclang.Core.Types;
using System.Collections.Generic;

namespace Libclang.Core.Ast
{
    public abstract class BaseDeclaration : IDeclaration
    {
        public string Name { get; set; }

        public virtual string FullName
        {
            get { return this.Name; }
        }

        public virtual IEnumerable<TypeDefinition> ReferedTypes
        {
            get
            {
                return Enumerable.Empty<TypeDefinition>();
            }
        }

        /// <summary>
        /// UnifiedSymbolResolution that uniquely identifies this declaration.
        /// </summary>
        internal string USR { get; set; }

        public bool IsDefinition { get; set; }

        public bool IsContainer { get; set; }

        public Location Location { get; set; }

        public DocumentDeclaration Document { get; set; }

        public PlatformAvailability IosAvailability { get; set; }

        public PlatformAvailability IosAppExtensionAvailability { get; set; }

        public IDeclaration Canonical
        {
            get
            {
                if (!(this is TypedefDeclaration))
                {
                    return this;
                }

                var typedefDeclaration = this as TypedefDeclaration;
                if (typedefDeclaration.UnderlyingType.Resolve() is DeclarationReferenceType)
                {
                    return (typedefDeclaration.UnderlyingType.Resolve() as DeclarationReferenceType).Target;
                }

                return null;
            }
        }

        protected BaseDeclaration(string name)
        {
            this.Name = name;
        }

        public bool IsSupported(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
            if (declarationsCache.ContainsKey(this))
            {
                return declarationsCache[this];
            }
            declarationsCache.Add(this, true);

            bool isSupported = this.IsSupportedInternal(typesCache, declarationsCache) ?? this.RefersOnlySupportedTypes(typesCache, declarationsCache);
            declarationsCache[this] = isSupported;

            return isSupported;
        }

        protected virtual bool? IsSupportedInternal(Dictionary<TypeDefinition, bool> typesCache, Dictionary<BaseDeclaration, bool> declarationsCache)
        {
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

        
    }
}

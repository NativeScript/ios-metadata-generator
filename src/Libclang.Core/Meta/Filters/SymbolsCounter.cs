using System;
using System.Linq;
using System.IO;
using Libclang.Core.Meta;
using Libclang.Core.Meta.Utils;
using Libclang.Core.Meta.Filters;
using System.Collections.Generic;

namespace Libclang.Core.Meta.Filters
{
    internal class SymbolsCounter : BaseMetaFilter
    {
        public int Structs { get; private set; }

        public int Unions { get; private set; }

        public int Functions { get; private set; }

        public int Vars { get; private set; }

        public int Enumerations { get; private set; }

        public int Protocols { get; private set; }

        public int Interfaces { get; private set; }

        public int Categories { get; private set; }

        public int Methods { get; private set; }

        public int PropertiesMethods { get; private set; } // Getters and setters

        public int Properties { get; private set; }

        public SymbolsCounter(TextWriter logger)
            : base(logger)
        {
        }

        protected override Action<MetaContainer, Libclang.Core.Meta.Meta, string> ActionForEach
        {
            get { return this.VisitMeta; }
        }

        public void VisitMeta(MetaContainer container, Libclang.Core.Meta.Meta meta, string key)
        {
            if (meta is StructMeta)
            {
                this.Structs++;
            }
            else if (meta is UnionMeta)
            {
                this.Unions++;
            }
            else if (meta is FunctionMeta)
            {
                this.Functions++;
            }
            else if (meta is VarMeta)
            {
                this.Vars++;
            }
            else if (meta is EnumMeta)
            {
                this.Enumerations++;
            }
            else if (meta is BaseClassMeta)
            {
                if (meta is InterfaceMeta)
                {
                    this.Interfaces++;
                    foreach (CategoryMeta category in ((InterfaceMeta) meta).Categories)
                    {
                        this.VisitMeta(container, category, null);
                    }
                }
                else if (meta is ProtocolMeta)
                {
                    this.Protocols++;
                }
                else if (meta is CategoryMeta)
                {
                    this.Categories++;
                }

                this.Methods += ((BaseClassMeta) meta).Methods.Count();

                foreach (PropertyMeta property in ((BaseClassMeta) meta).Properties)
                {
                    this.Properties++;
                    if (property.Getter != null)
                    {
                        this.PropertiesMethods++;
                    }
                    if (property.Setter != null)
                    {
                        this.PropertiesMethods++;
                    }
                }
            }
        }

        protected override void End(MetaContainer metaContainer)
        {
            int topLevelSymbols = this.Structs + this.Unions + this.Functions + this.Vars + this.Enumerations +
                                  this.Interfaces + this.Protocols;
            int allSymbols = topLevelSymbols + this.Methods + this.Properties + this.PropertiesMethods;

            this.Log("Structs: {0}", this.Structs);
            this.Log("Unions: {0}", this.Unions);
            this.Log("Functinos: {0}", this.Functions);
            this.Log("Vars: {0}", this.Vars);
            this.Log("Enumerations: {0}", this.Enumerations);
            this.Log("Interfaces: {0}", this.Interfaces);
            this.Log("Protocols: {0}", this.Protocols);
            this.Log("Methods: {0}", this.Methods);
            this.Log("Properties Methods(getters and setters): {0}", this.PropertiesMethods);
            this.Log("Properties: {0}", this.Properties);
            this.Log("-------------------------");
            this.Log("Top Level Symbols: {0}", topLevelSymbols);
            this.Log("All Symbols: {0}", allSymbols);
            this.Log("(Categories: {0})", this.Categories);
        }
    }
}

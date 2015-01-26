using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libclang.Core.Ast;
using Libclang.Core.Generator;
using Libclang.Core.Meta.Filters;

namespace Libclang.Core.Meta.Utils
{
    public class MetaContainer : IEnumerable<KeyValuePair<string, Meta>>
    {
        private readonly Dictionary<string, Meta> container;

        public MetaContainer()
        {
            this.Duplicates = new SymbolsNamesCollection();
            this.container = new Dictionary<string, Meta>();
            this.JsNameGenerator = new DefaultJsNameGenerator();
        }

        public SymbolsNamesCollection Duplicates { get; private set; }

        public IJsNameGenerator JsNameGenerator { get; private set; }

        public Meta this[string key]
        {
            get { return this.container[key]; }
        }

        public bool ContainsKey(string key)
        {
            return this.container.ContainsKey(key);
        }

        public bool TryGetMeta(string key, out Meta meta)
        {
            return this.container.TryGetValue(key, out meta);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<string, Meta>> GetEnumerator()
        {
            return this.container.GetEnumerator();
        }

        private Meta CreateMeta(BaseDeclaration declaration)
        {
            if (declaration is StructDeclaration)
            {
                return MetaFactory.CreateStruct(new StructMeta(), (StructDeclaration) declaration, this);
            }
            else if (declaration is UnionDeclaration)
            {
                return MetaFactory.CreateUnion(new UnionMeta(), (UnionDeclaration) declaration, this);
            }
            else if (declaration is EnumDeclaration)
            {
                return MetaFactory.CreateEnum(new EnumMeta(), (EnumDeclaration) declaration, this);
            }
            else if (declaration is VarDeclaration)
            {
                return MetaFactory.CreateVar(new VarMeta(), (VarDeclaration) declaration, this);
            }
            else if (declaration is FunctionDeclaration)
            {
                return MetaFactory.CreateFunction(new FunctionMeta(), (FunctionDeclaration) declaration, this);
            }
            else if (declaration is ProtocolDeclaration)
            {
                return MetaFactory.CreateProtocol(new ProtocolMeta(), (ProtocolDeclaration) declaration, this);
            }
            else if (declaration is InterfaceDeclaration)
            {
                return MetaFactory.CreateInterface(new InterfaceMeta(), (InterfaceDeclaration) declaration, this);
            }
            else
            {
                throw new ArgumentException("Not supported meta type.");
            }
        }

        public void AddMeta(Meta meta)
        {
            if (meta.JSName == null)
            {
                throw new Exception("A meta object with null JS name can't be added to MetaContainer.");
            }
            if (this.ContainsKey(meta.JSName))
            {
                throw new Exception(
                    String.Format("The MetaContainer already contains meta object with JSName equal to '{0}'",
                        meta.JSName));
            }
            this.container.Add(meta.JSName, meta);
        }

        public void AddDeclaration(BaseDeclaration declaration)
        {
            Meta meta = this.CreateMeta(declaration);
            AddMeta(meta);
        }

        public void AddDeclarations(params IEnumerable<BaseDeclaration>[] declarationsCollections)
        {
            foreach (IEnumerable<BaseDeclaration> declarations in declarationsCollections)
            {
                foreach (BaseDeclaration declaration in declarations)
                {
                    this.AddDeclaration(declaration);
                }
            }
        }

        public bool Remove(string key)
        {
            return this.container.Remove(key);
        }

        public string CalculateJsName(BaseDeclaration declaration)
        {
            string jsName = this.JsNameGenerator.GenerateJsName(declaration);

            if (this.Duplicates.Contains(jsName, declaration.GetType()))
            {
                jsName = this.JsNameGenerator.TryResolveConflict(jsName, declaration);
            }

            return jsName;
        }

        public int Count
        {
            get { return this.container.Count; }
        }

        public void Filter(params IMetaFilter[] filters)
        {
            foreach (IMetaFilter filter in filters)
            {
                filter.Filter(this);
            }
        }
    }
}

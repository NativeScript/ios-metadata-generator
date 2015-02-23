using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGenerator.Core.Ast
{
    public class ModuleDeclaration : BaseDeclaration
    {
        private string fullName;

        public ModuleDeclaration Parent { get; set; }

        public IList<ModuleDeclaration> Submodules { get; private set; }

        public decimal Version { get; set; }

        public override string FullName
        {
            get
            {
                return this.fullName;
            }
        }

        public IList<IDeclaration> Declarations { get; private set; }

        //public ICollection<KeyValuePair<string, Types.DeclarationReferenceType>> UnresolvedDeclarationReferences { get; set; }

        public IEnumerable<TypedefDeclaration> Typedefs
        {
            get { return this.Declarations.OfType<TypedefDeclaration>(); }
        }

        public IEnumerable<VarDeclaration> Vars
        {
            get { return this.Declarations.OfType<VarDeclaration>(); }
        }

        public IEnumerable<StructDeclaration> Structs
        {
            get { return this.Declarations.OfType<StructDeclaration>(); }
        }

        public IEnumerable<UnionDeclaration> Unions
        {
            get { return this.Declarations.OfType<UnionDeclaration>(); }
        }

        public IEnumerable<BaseRecordDeclaration> Records
        {
            get { return this.Declarations.OfType<BaseRecordDeclaration>(); }
        }

        public IEnumerable<EnumDeclaration> Enums
        {
            get { return this.Declarations.OfType<EnumDeclaration>(); }
        }

        public IEnumerable<FunctionDeclaration> Functions
        {
            get { return this.Declarations.OfType<FunctionDeclaration>(); }
        }

        public IEnumerable<InterfaceDeclaration> Interfaces
        {
            get { return this.Declarations.OfType<InterfaceDeclaration>(); }
        }

        public IEnumerable<ProtocolDeclaration> Protocols
        {
            get { return this.Declarations.OfType<ProtocolDeclaration>(); }
        }

        public IEnumerable<CategoryDeclaration> Categories
        {
            get { return this.Declarations.OfType<CategoryDeclaration>(); }
        }

        public IEnumerable<MethodDeclaration> Methods
        {
            get { return this.Declarations.OfType<BaseClass>().SelectMany(x => x.Methods); }
        }

        public ModuleDeclaration(string name)
            : this(name, name)
        { }

        public ModuleDeclaration(string name, string fullName)
            : base(name)
        {
            this.Declarations = new List<IDeclaration>();
            this.Submodules = new List<ModuleDeclaration>();
            this.fullName = fullName;
        }

        public void Add(IDeclaration element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            this.Declarations.Add(element);
        }

#if DEBUG
        public override string ToString()
        {
            return "MODULE_DECLARATION: " + this.fullName;
        }
#endif

        public override void Accept(Meta.Visitors.IDeclarationVisitor visitor)
        {
            foreach (var declaration in this.Declarations)
            {
                declaration.Accept(visitor);
            }
            foreach (var submodule in this.Submodules)
            {
                submodule.Accept(visitor);
            }
            visitor.Visit(this);
        }
    }
}

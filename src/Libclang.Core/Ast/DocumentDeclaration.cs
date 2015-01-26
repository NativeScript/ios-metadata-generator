using System;
using System.Collections.Generic;
using System.Linq;

namespace Libclang.Core.Ast
{
    public class DocumentDeclaration : BaseDeclaration
    {
        public decimal Version { get; set; }

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

        public DocumentDeclaration(string name)
            : base(name)
        {
            this.Declarations = new List<IDeclaration>();
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
            return "DOCUMENT_DECLARATION: " + this.Name +
                   string.Concat(this.Declarations.Select(x => Environment.NewLine + x));
        }
#endif
    }
}

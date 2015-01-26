using Libclang.Core.Ast;

namespace Libclang.Core.Generator
{
    public abstract class BaseWriter : DeclarationVisitor
    {
        protected IFormatter formatter;

        protected BaseWriter(DocumentDeclaration documentDeclaration, IFormatter formatter)
            : base(documentDeclaration)
        {
            this.formatter = formatter;
        }

        public virtual void Generate()
        {
            VisitAll();
        }
    }
}

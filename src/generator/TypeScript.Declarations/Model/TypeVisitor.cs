namespace TypeScript.Declarations.Model
{
    public abstract class TypeVisitor
    {
        protected internal virtual void VisitAny()
        {
        }

        protected internal virtual void VisitString()
        {
        }

        protected internal virtual void VisitNumber()
        {
        }

        protected internal virtual void VisitBoolean()
        {
        }

        protected internal virtual void VisitVoid()
        {
        }

        protected internal virtual void Visit(ClassDeclaration classDeclaration)
        {
        }

        protected internal virtual void Visit(InterfaceDeclaration interfaceDeclaration)
        {
        }

        protected internal virtual void Visit(ArrayType array)
        {
        }

        protected internal virtual void Visit(ObjectType objectType)
        {
        }

        protected internal virtual void Visit(FunctionType functionType)
        {
        }
    }
}

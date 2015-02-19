namespace TypeScript.Declarations.Writers
{
    using TypeScript.Declarations.Model;

    internal partial class TypeWriter : TypeVisitor
    {
        protected internal override void VisitAny()
        {
            this.WriteAny();
        }

        protected internal override void VisitString()
        {
            this.WriteString();
        }

        protected internal override void VisitNumber()
        {
            this.WriteNumber();
        }

        protected internal override void VisitBoolean()
        {
            this.WriteBoolean();
        }

        protected internal override void VisitVoid()
        {
            this.WriteVoid();
        }

        protected internal override void Visit(ClassDeclaration classDeclaration)
        {
            this.WriteClass(classDeclaration);
        }

        protected internal override void Visit(InterfaceDeclaration interfaceDeclaration)
        {
            this.WriteInterface(interfaceDeclaration);
        }

        protected internal override void Visit(ArrayType array)
        {
            this.WriteArray(array);
        }

        protected internal override void Visit(ObjectType objectType)
        {
            this.WriteObjectType(objectType);
        }

        protected internal override void Visit(FunctionType functionType)
        {
            this.WriteFunctionType(functionType);
        }
    }
}

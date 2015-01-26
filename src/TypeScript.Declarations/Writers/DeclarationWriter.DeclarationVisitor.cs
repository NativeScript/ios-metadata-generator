namespace TypeScript.Declarations.Writers
{
    using TypeScript.Declarations.Model;

    internal partial class DeclarationWriter : DeclarationVisitor
    {
        protected internal override void Visit(ClassDeclaration classDeclaration)
        {
            this.WriteClass(classDeclaration);
        }

        protected internal override void Visit(InterfaceDeclaration interfaceDeclaration)
        {
            this.WriteInterface(interfaceDeclaration);
        }

        protected internal override void Visit(EnumDeclaration enumDeclaration)
        {
            this.WriteEnum(enumDeclaration);
        }

        protected internal override void Visit(VariableStatement variableStatement)
        {
            this.WriteVar(variableStatement);
        }

        protected internal override void Visit(FunctionDeclaration functionDeclaration)
        {
            this.WriteFunction(functionDeclaration);
        }
    }
}

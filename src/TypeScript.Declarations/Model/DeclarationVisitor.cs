namespace TypeScript.Declarations.Model
{
    public abstract class DeclarationVisitor
    {
        protected internal virtual void Visit(SourceUnit sourceUnit)
        {
            if (sourceUnit == null)
            {
                throw new System.ArgumentNullException("sourceUnit");
            }

            foreach (var item in sourceUnit.Children)
            {
                item.Accept(this);
            }
        }

        protected internal virtual void Visit(ClassDeclaration classDeclaration)
        {
        }

        protected internal virtual void Visit(EnumDeclaration enumDeclaration)
        {
        }

        protected internal virtual void Visit(FunctionDeclaration functionDeclaration)
        {
        }

        protected internal virtual void Visit(InterfaceDeclaration interfaceDeclaration)
        {
        }

        protected internal virtual void Visit(VariableStatement variableStatement)
        {
        }
    }
}

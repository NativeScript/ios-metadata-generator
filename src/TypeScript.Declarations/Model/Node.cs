namespace TypeScript.Declarations.Model
{
    public abstract class Declaration : TypeScriptObject
    {
        public abstract void Accept(DeclarationVisitor visitor);
    }
}

namespace TypeScript.Declarations.Model
{
    public class VariableStatement : Declaration
    {
        public string Name { get; set; }

        public IType TypeAnnotation { get; set; }

        public override void Accept(DeclarationVisitor visitor)
        {
            if (visitor == null)
            {
                throw new System.ArgumentNullException("visitor");
            }

            visitor.Visit(this);
        }
    }
}

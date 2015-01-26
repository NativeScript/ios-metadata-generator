namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;

    public class FunctionDeclaration : Declaration
    {
        public FunctionDeclaration()
        {
            this.Parameters = new List<Parameter>();
        }

        public string Name { get; set; }

        public IType ReturnType { get; set; }

        public IList<Parameter> Parameters { get; private set; }

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

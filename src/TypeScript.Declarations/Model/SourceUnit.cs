namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;

    public class SourceUnit : Declaration
    {
        public SourceUnit()
        {
            this.Children = new List<Declaration>();
        }

        public IList<Declaration> Children { get; private set; }

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

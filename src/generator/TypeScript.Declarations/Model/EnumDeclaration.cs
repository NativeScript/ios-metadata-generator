namespace TypeScript.Declarations.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("class {Name}")]
    public class EnumDeclaration : Declaration
    {
        public EnumDeclaration()
        {
            this.Members = new List<EnumElement>();
        }

        public string Name { get; set; }

        public IList<EnumElement> Members { get; private set; }

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
